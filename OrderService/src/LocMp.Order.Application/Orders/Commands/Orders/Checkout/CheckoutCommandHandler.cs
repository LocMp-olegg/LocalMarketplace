using AutoMapper;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Contracts.Orders;
using LocMp.Order.Application.DTOs;
using LocMp.Order.Domain.Entities;
using LocMp.Order.Domain.Enums;
using LocMp.Order.Infrastructure.DTOs;
using LocMp.Order.Infrastructure.Interfaces;
using LocMp.Order.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using OrderEntity = LocMp.Order.Domain.Entities.Order;

namespace LocMp.Order.Application.Orders.Commands.Orders.Checkout;

public sealed class CheckoutCommandHandler(
    OrderDbContext db,
    ICatalogClient catalogClient,
    IEventBus eventBus,
    IMapper mapper)
    : IRequestHandler<CheckoutCommand, IReadOnlyList<OrderDto>>
{
    public async Task<IReadOnlyList<OrderDto>> Handle(CheckoutCommand request, CancellationToken ct)
    {
        var cart = await db.Carts
                       .Include(c => c.Items)
                       .FirstOrDefaultAsync(c => c.UserId == request.UserId && c.ExpiresAt > DateTimeOffset.UtcNow, ct)
                   ?? throw new NotFoundException("Active cart not found.");

        if (!cart.Items.Any())
            throw new ConflictException("Cart is empty.");

        // Collect items that will actually be checked out
        var allSelectedItems = new List<CartItem>();
        foreach (var groupSettings in request.Groups)
        {
            var groupKey = (groupSettings.SellerId, groupSettings.ShopId);
            var groupItems = cart.Items
                .Where(i => i.SellerId == groupSettings.SellerId && i.ShopId == groupSettings.ShopId)
                .ToList();

            if (!groupItems.Any())
                throw new ConflictException(
                    $"No cart items found for seller '{groupSettings.SellerId}' / shop '{groupSettings.ShopId}'.");

            if (groupSettings.SelectedItemIds is { Count: > 0 })
            {
                var selectedSet = groupSettings.SelectedItemIds.ToHashSet();
                groupItems = groupItems.Where(i => selectedSet.Contains(i.Id)).ToList();
                if (!groupItems.Any())
                    throw new ConflictException(
                        $"None of the selected items belong to seller '{groupSettings.SellerId}' / shop '{groupSettings.ShopId}'.");
            }

            allSelectedItems.AddRange(groupItems);
        }

        // Detect duplicate groups in request
        var requestedKeys = request.Groups.Select(g => (g.SellerId, g.ShopId)).ToList();
        if (requestedKeys.Distinct().Count() != request.Groups.Count)
            throw new ConflictException("Duplicate seller/shop groups in checkout request.");

        // Fetch product snapshots only for items being checked out
        var productResults = await Task.WhenAll(
            allSelectedItems.Select(item => catalogClient.GetProductAsync(item.ProductId, ct)));

        var snapshots = new Dictionary<Guid, ProductSnapshotDto>(allSelectedItems.Count);
        for (var i = 0; i < allSelectedItems.Count; i++)
        {
            var cartItem = allSelectedItems[i];
            var product = productResults[i]
                          ?? throw new ConflictException($"Product '{cartItem.ProductId}' is no longer available.");

            if (!product.IsActive)
                throw new ConflictException($"Product '{product.Name}' is not active.");

            if (!product.IsMadeToOrder && product.StockQuantity < cartItem.Quantity)
                throw new ConflictException(
                    $"Insufficient stock for '{product.Name}'. Available: {product.StockQuantity}, requested: {cartItem.Quantity}.");

            snapshots[cartItem.ProductId] = product;
        }

        // Validate courier delivery is allowed for shops that requested NeighborCourier
        var courierGroups = request.Groups
            .Where(g => g.DeliveryType == DeliveryType.NeighborCourier && g.ShopId.HasValue)
            .ToList();

        if (courierGroups.Count > 0)
        {
            var shopSettings = await Task.WhenAll(
                courierGroups.Select(g => catalogClient.GetShopDeliverySettingsAsync(g.ShopId!.Value, ct)));

            for (var i = 0; i < courierGroups.Count; i++)
            {
                if (shopSettings[i] is { AllowCourierDelivery: false })
                    throw new ConflictException(
                        $"Shop '{courierGroups[i].ShopId}' does not allow courier delivery.");
            }
        }

        // Group cart items by (SellerId, ShopId) — now directly from CartItem fields
        var cartGroups = allSelectedItems
            .GroupBy(item => (item.SellerId, item.ShopId))
            .ToDictionary(g => g.Key, g => g.ToList());

        var checkoutId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        var createdOrders = new List<OrderEntity>(request.Groups.Count);
        var allItemsForReservation = new List<(Guid ProductId, int Quantity, Guid OrderId)>();
        var checkedOutCartItemIds = allSelectedItems.Select(i => i.Id).ToHashSet();

        foreach (var groupSettings in request.Groups)
        {
            var groupKey = (groupSettings.SellerId, groupSettings.ShopId);
            var groupCartItems = cartGroups[groupKey];
            var orderId = Guid.NewGuid();

            var firstItem = groupCartItems[0];

            var order = new OrderEntity(orderId)
            {
                CheckoutId   = checkoutId,
                BuyerId      = request.UserId,
                SellerId     = groupSettings.SellerId,
                SellerName   = firstItem.SellerName,
                ShopId       = groupSettings.ShopId,
                ShopName     = firstItem.ShopName,
                DeliveryType = groupSettings.DeliveryType,
                BuyerComment = request.BuyerComment,
                CreatedAt    = now
            };

            var orderItems = groupCartItems.Select(cartItem =>
            {
                var snap = snapshots[cartItem.ProductId];
                return new OrderItem(Guid.NewGuid())
                {
                    OrderId            = orderId,
                    ProductId          = cartItem.ProductId,
                    ProductName        = snap.Name,
                    ProductDescription = snap.Description,
                    MainPhotoUrl       = snap.MainPhotoUrl,
                    ShopId             = snap.ShopId,
                    ShopName           = snap.ShopName,
                    UnitPrice          = snap.Price,
                    Quantity           = cartItem.Quantity,
                    Subtotal           = snap.Price * cartItem.Quantity
                };
            }).ToList();

            order.TotalAmount = orderItems.Sum(i => i.Subtotal);

            var statusEntry = new OrderStatusHistory(Guid.NewGuid())
            {
                OrderId     = orderId,
                FromStatus  = null,
                ToStatus    = OrderStatus.Pending,
                ChangedById = request.UserId,
                ChangedAt   = now
            };

            if (groupSettings.DeliveryType == DeliveryType.NeighborCourier &&
                groupSettings.DeliveryAddress is { } addr)
            {
                Point? location = null;
                if (addr.Latitude.HasValue && addr.Longitude.HasValue)
                    location = new Point(addr.Longitude.Value, addr.Latitude.Value) { SRID = 4326 };

                order.DeliveryAddress = new DeliveryAddress(Guid.NewGuid())
                {
                    OrderId       = orderId,
                    City          = addr.City,
                    Street        = addr.Street,
                    HouseNumber   = addr.HouseNumber,
                    Apartment     = addr.Apartment,
                    Entrance      = addr.Entrance,
                    Floor         = addr.Floor,
                    Location      = location,
                    RecipientName = addr.RecipientName,
                    RecipientPhone = addr.RecipientPhone
                };
            }

            db.Orders.Add(order);
            db.OrderItems.AddRange(orderItems);
            db.OrderStatusHistory.Add(statusEntry);

            createdOrders.Add(order);
            allItemsForReservation.AddRange(
                groupCartItems.Select(i => (i.ProductId, i.Quantity, orderId)));
        }

        // Remove only checked-out items (partial checkout support)
        var itemsToRemove = cart.Items.Where(i => checkedOutCartItemIds.Contains(i.Id)).ToList();
        db.CartItems.RemoveRange(itemsToRemove);
        if (cart.Items.All(i => checkedOutCartItemIds.Contains(i.Id)))
            db.Carts.Remove(cart);

        await db.SaveChangesAsync(ct);

        // Reserve stock for all orders; rollback everything on failure
        var reserved = new List<(Guid ProductId, int Quantity, Guid OrderId)>(allItemsForReservation.Count);
        try
        {
            foreach (var (productId, quantity, orderId) in allItemsForReservation)
            {
                await catalogClient.ReserveStockAsync(productId, quantity, orderId, ct);
                reserved.Add((productId, quantity, orderId));
            }
        }
        catch
        {
            foreach (var (productId, quantity, orderId) in reserved)
            {
                try { await catalogClient.ReleaseStockAsync(productId, quantity, orderId, CancellationToken.None); }
                catch { /* best-effort */ }
            }

            await transaction.RollbackAsync(ct);
            throw new ConflictException("Stock reservation failed. Please try again.");
        }

        await transaction.CommitAsync(ct);

        foreach (var order in createdOrders)
        {
            await eventBus.PublishAsync(
                new OrderPlacedEvent(order.Id, request.UserId, order.SellerId, order.TotalAmount, now), ct);
        }

        var orderIds = createdOrders.Select(o => o.Id).ToList();
        var result = await db.Orders
            .Include(o => o.Items)
            .Include(o => o.StatusHistory)
            .Include(o => o.Photos)
            .Include(o => o.DeliveryAddress)
            .Include(o => o.CourierAssignment)
            .Include(o => o.Dispute)
            .Where(o => orderIds.Contains(o.Id))
            .ToListAsync(ct);

        return result.Select(mapper.Map<OrderDto>).ToList();
    }
}
