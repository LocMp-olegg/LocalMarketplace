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
using CartEntity = LocMp.Order.Domain.Entities.Cart;
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
        var cart = await LoadCartAsync(request.UserId, ct);
        var selectedItems = CollectSelectedItems(cart, request.Groups);
        ValidateNoDuplicateGroups(request.Groups);

        var snapshots = await FetchAndValidateSnapshotsAsync(selectedItems, ct);
        await ValidateCourierDeliveryAsync(request.Groups, ct);

        var createdOrders = await CreateOrdersInTransactionAsync(
            request, cart, selectedItems, snapshots, ct);

        await PublishOrderEventsAsync(createdOrders, request.UserId, ct);

        return await LoadCreatedOrdersAsync(createdOrders.Select(o => o.Id).ToList(), ct);
    }

    private async Task<CartEntity> LoadCartAsync(Guid userId, CancellationToken ct)
    {
        var cart = await db.Carts
                       .Include(c => c.Items)
                       .FirstOrDefaultAsync(c => c.UserId == userId && c.ExpiresAt > DateTimeOffset.UtcNow, ct)
                   ?? throw new NotFoundException("Active cart not found.");

        if (!cart.Items.Any())
            throw new ConflictException("Cart is empty.");

        return cart;
    }

    private static List<CartItem> CollectSelectedItems(CartEntity cart, IReadOnlyList<GroupDeliverySettings> groups)
    {
        var allSelected = new List<CartItem>();

        foreach (var group in groups)
        {
            var groupItems = cart.Items
                .Where(i => i.SellerId == group.SellerId && i.ShopId == group.ShopId)
                .ToList();

            if (!groupItems.Any())
                throw new ConflictException(
                    $"No cart items found for seller '{group.SellerId}' / shop '{group.ShopId}'.");

            if (group.SelectedItemIds is { Count: > 0 })
            {
                var selectedSet = group.SelectedItemIds.ToHashSet();
                groupItems = groupItems.Where(i => selectedSet.Contains(i.Id)).ToList();

                if (!groupItems.Any())
                    throw new ConflictException(
                        $"None of the selected items belong to seller '{group.SellerId}' / shop '{group.ShopId}'.");
            }

            allSelected.AddRange(groupItems);
        }

        return allSelected;
    }

    private static void ValidateNoDuplicateGroups(IReadOnlyList<GroupDeliverySettings> groups)
    {
        var keys = groups.Select(g => (g.SellerId, g.ShopId)).ToList();
        if (keys.Distinct().Count() != groups.Count)
            throw new ConflictException("Duplicate seller/shop groups in checkout request.");
    }

    private async Task<Dictionary<Guid, ProductSnapshotDto>> FetchAndValidateSnapshotsAsync(
        List<CartItem> items, CancellationToken ct)
    {
        var productResults = await Task.WhenAll(
            items.Select(i => catalogClient.GetProductAsync(i.ProductId, ct)));

        var snapshots = new Dictionary<Guid, ProductSnapshotDto>(items.Count);

        for (var i = 0; i < items.Count; i++)
        {
            var cartItem = items[i];
            var product = productResults[i]
                          ?? throw new ConflictException($"Product '{cartItem.ProductId}' is no longer available.");

            if (!product.IsActive)
                throw new ConflictException($"Product '{product.Name}' is not active.");

            if (!product.IsMadeToOrder && product.StockQuantity < cartItem.Quantity)
                throw new ConflictException(
                    $"Insufficient stock for '{product.Name}'. Available: {product.StockQuantity}, requested: {cartItem.Quantity}.");

            snapshots[cartItem.ProductId] = product;
        }

        return snapshots;
    }

    private async Task ValidateCourierDeliveryAsync(IReadOnlyList<GroupDeliverySettings> groups, CancellationToken ct)
    {
        var courierGroups = groups
            .Where(g => g.DeliveryType == DeliveryType.NeighborCourier && g.ShopId.HasValue)
            .ToList();

        if (courierGroups.Count == 0) return;

        var shopSettings = await Task.WhenAll(
            courierGroups.Select(g => catalogClient.GetShopDeliverySettingsAsync(g.ShopId!.Value, ct)));

        for (var i = 0; i < courierGroups.Count; i++)
        {
            var group = courierGroups[i];
            var settings = shopSettings[i];

            if (settings is { AllowCourierDelivery: false })
                throw new ConflictException($"Shop '{group.ShopId}' does not allow courier delivery.");

            if (settings is { MaxCourierDistanceMeters: not null, Latitude: not null, Longitude: not null }
                && group.DeliveryAddress is { Latitude: not null, Longitude: not null })
            {
                var distanceMeters = CalculateDistanceMeters(
                    settings.Latitude.Value, settings.Longitude.Value,
                    group.DeliveryAddress.Latitude.Value, group.DeliveryAddress.Longitude.Value);

                if (distanceMeters > settings.MaxCourierDistanceMeters.Value)
                    throw new ConflictException(
                        $"Delivery address is {(int)distanceMeters / 1000.0:F1} km away. " +
                        $"Shop '{group.ShopId}' only delivers within {settings.MaxCourierDistanceMeters.Value / 1000.0:F1} km.");
            }
        }
    }

    private static double CalculateDistanceMeters(
        double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusMeters = 6_371_000;
        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2))
              * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return earthRadiusMeters * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double ToRad(double degrees) => degrees * Math.PI / 180;

    private async Task<List<OrderEntity>> CreateOrdersInTransactionAsync(
        CheckoutCommand request,
        CartEntity cart,
        List<CartItem> selectedItems,
        Dictionary<Guid, ProductSnapshotDto> snapshots,
        CancellationToken ct)
    {
        var cartGroups = selectedItems
            .GroupBy(i => (i.SellerId, i.ShopId))
            .ToDictionary(g => g.Key, g => g.ToList());

        var checkoutId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var checkedOutIds = selectedItems.Select(i => i.Id).ToHashSet();

        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        var createdOrders = new List<OrderEntity>(request.Groups.Count);
        var reservationItems = new List<(Guid, int, Guid)>();

        foreach (var group in request.Groups)
        {
            var groupCartItems = cartGroups[(group.SellerId, group.ShopId)];
            var (order, orderItems, statusEntry) =
                BuildOrder(group, groupCartItems, snapshots, checkoutId, request, now);

            db.Orders.Add(order);
            db.OrderItems.AddRange(orderItems);
            db.OrderStatusHistory.Add(statusEntry);

            createdOrders.Add(order);
            reservationItems.AddRange(groupCartItems.Select(i => (i.ProductId, i.Quantity, order.Id)));
        }

        RemoveCheckedOutCartItems(cart, checkedOutIds);
        await db.SaveChangesAsync(ct);

        try
        {
            await ReserveStockWithRollbackAsync(reservationItems, ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }

        return createdOrders;
    }

    private static (OrderEntity Order, List<OrderItem> Items, OrderStatusHistory StatusEntry) BuildOrder(
        GroupDeliverySettings group,
        List<CartItem> cartItems,
        Dictionary<Guid, ProductSnapshotDto> snapshots,
        Guid checkoutId,
        CheckoutCommand request,
        DateTimeOffset now)
    {
        var orderId = Guid.NewGuid();
        var firstItem = cartItems[0];

        var order = new OrderEntity(orderId)
        {
            CheckoutId = checkoutId,
            BuyerId = request.UserId,
            SellerId = group.SellerId,
            SellerName = firstItem.SellerName,
            ShopId = group.ShopId,
            ShopName = firstItem.ShopName,
            DeliveryType = group.DeliveryType,
            BuyerComment = request.BuyerComment,
            CreatedAt = now
        };

        var orderItems = cartItems.Select(cartItem =>
        {
            var snap = snapshots[cartItem.ProductId];
            return new OrderItem(Guid.NewGuid())
            {
                OrderId = orderId,
                ProductId = cartItem.ProductId,
                ProductName = snap.Name,
                ProductDescription = snap.Description,
                MainPhotoUrl = snap.MainPhotoUrl,
                ShopId = snap.ShopId,
                ShopName = snap.ShopName,
                UnitPrice = snap.Price,
                Quantity = cartItem.Quantity,
                Subtotal = snap.Price * cartItem.Quantity
            };
        }).ToList();

        order.TotalAmount = orderItems.Sum(i => i.Subtotal);

        var statusEntry = new OrderStatusHistory(Guid.NewGuid())
        {
            OrderId = orderId,
            FromStatus = null,
            ToStatus = OrderStatus.Pending,
            ChangedById = request.UserId,
            ChangedAt = now
        };

        if (group.DeliveryType == DeliveryType.NeighborCourier && group.DeliveryAddress is { } addr)
            order.DeliveryAddress = BuildDeliveryAddress(orderId, addr);

        return (order, orderItems, statusEntry);
    }

    private static DeliveryAddress BuildDeliveryAddress(Guid orderId, DeliveryAddressData addr)
    {
        Point? location = null;
        if (addr.Latitude.HasValue && addr.Longitude.HasValue)
            location = new Point(addr.Longitude.Value, addr.Latitude.Value) { SRID = 4326 };

        return new DeliveryAddress(Guid.NewGuid())
        {
            OrderId = orderId,
            City = addr.City,
            Street = addr.Street,
            HouseNumber = addr.HouseNumber,
            Apartment = addr.Apartment,
            Entrance = addr.Entrance,
            Floor = addr.Floor,
            Location = location,
            RecipientName = addr.RecipientName,
            RecipientPhone = addr.RecipientPhone
        };
    }

    private void RemoveCheckedOutCartItems(CartEntity cart, HashSet<Guid> checkedOutIds)
    {
        var toRemove = cart.Items.Where(i => checkedOutIds.Contains(i.Id)).ToList();
        db.CartItems.RemoveRange(toRemove);

        if (cart.Items.All(i => checkedOutIds.Contains(i.Id)))
            db.Carts.Remove(cart);
    }

    private async Task ReserveStockWithRollbackAsync(
        List<(Guid ProductId, int Quantity, Guid OrderId)> items, CancellationToken ct)
    {
        var reserved = new List<(Guid ProductId, int Quantity, Guid OrderId)>(items.Count);
        try
        {
            foreach (var item in items)
            {
                await catalogClient.ReserveStockAsync(item.ProductId, item.Quantity, item.OrderId, ct);
                reserved.Add(item);
            }
        }
        catch
        {
            foreach (var item in reserved)
            {
                try
                {
                    await catalogClient.ReleaseStockAsync(item.ProductId, item.Quantity, item.OrderId,
                        CancellationToken.None);
                }
                catch
                {
                    /* best-effort */
                }
            }

            throw new ConflictException("Stock reservation failed. Please try again.");
        }
    }

    private async Task PublishOrderEventsAsync(List<OrderEntity> orders, Guid buyerId, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var order in orders)
            await eventBus.PublishAsync(
                new OrderPlacedEvent(order.Id, buyerId, order.SellerId, order.TotalAmount, now), ct);
    }

    private async Task<IReadOnlyList<OrderDto>> LoadCreatedOrdersAsync(List<Guid> orderIds, CancellationToken ct)
    {
        var orders = await db.Orders
            .Include(o => o.Items)
            .Include(o => o.StatusHistory)
            .Include(o => o.Photos)
            .Include(o => o.DeliveryAddress)
            .Include(o => o.CourierAssignment)
            .Include(o => o.Dispute)
            .Where(o => orderIds.Contains(o.Id))
            .ToListAsync(ct);

        return orders.Select(mapper.Map<OrderDto>).ToList();
    }
}