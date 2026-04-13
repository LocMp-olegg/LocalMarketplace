using AutoMapper;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Contracts.Orders;
using LocMp.Order.Application.DTOs;
using LocMp.Order.Infrastructure.DTOs;
using LocMp.Order.Infrastructure.Interfaces;
using LocMp.Order.Domain.Entities;
using LocMp.Order.Domain.Enums;
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
    : IRequestHandler<CheckoutCommand, OrderDto>
{
    public async Task<OrderDto> Handle(CheckoutCommand request, CancellationToken ct)
    {
        var cart = await db.Carts
                       .Include(c => c.Items)
                       .FirstOrDefaultAsync(c => c.UserId == request.UserId && c.ExpiresAt > DateTimeOffset.UtcNow, ct)
                   ?? throw new NotFoundException("Active cart not found.");

        if (!cart.Items.Any())
            throw new ConflictException("Cart is empty.");

        // Fetch all products in parallel instead of sequential HTTP calls
        var productResults = await Task.WhenAll(
            cart.Items.Select(item => catalogClient.GetProductAsync(item.ProductId, ct)));

        var snapshots = new Dictionary<Guid, ProductSnapshotDto>(cart.Items.Count);
        for (var i = 0; i < cart.Items.Count; i++)
        {
            var cartItem = cart.Items.ElementAt(i);
            var product = productResults[i]
                          ?? throw new ConflictException($"Product '{cartItem.ProductId}' is no longer available.");

            if (!product.IsActive)
                throw new ConflictException($"Product '{product.Name}' is not active.");

            if (product.StockQuantity < cartItem.Quantity)
                throw new ConflictException(
                    $"Insufficient stock for '{product.Name}'. Available: {product.StockQuantity}, requested: {cartItem.Quantity}.");

            snapshots[cartItem.ProductId] = product;
        }

        // All sellers must be the same (one order per seller)
        var sellerIds = snapshots.Values.Select(p => p.SellerId).Distinct().ToList();
        if (sellerIds.Count > 1)
            throw new ConflictException("Cart contains products from multiple sellers. Please checkout separately.");

        var sellerId = sellerIds[0];
        var orderId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        var order = new OrderEntity(orderId)
        {
            BuyerId = request.UserId,
            SellerId = sellerId,
            DeliveryType = request.DeliveryType,
            BuyerComment = request.BuyerComment,
            CreatedAt = now
        };

        var items = cart.Items.Select(cartItem =>
        {
            var snap = snapshots[cartItem.ProductId];
            return new OrderItem(Guid.NewGuid())
            {
                OrderId = orderId,
                ProductId = cartItem.ProductId,
                ProductName = snap.Name,
                ProductDescription = snap.Description,
                MainPhotoUrl = snap.MainPhotoUrl,
                UnitPrice = snap.Price,
                Quantity = cartItem.Quantity,
                Subtotal = snap.Price * cartItem.Quantity
            };
        }).ToList();

        order.TotalAmount = items.Sum(i => i.Subtotal);

        var statusEntry = new OrderStatusHistory(Guid.NewGuid())
        {
            OrderId = orderId,
            FromStatus = null,
            ToStatus = OrderStatus.Pending,
            ChangedById = request.UserId,
            ChangedAt = now
        };

        if (request.DeliveryType == DeliveryType.NeighborCourier && request.DeliveryAddress is { } addr)
        {
            Point? location = null;
            if (addr.Latitude.HasValue && addr.Longitude.HasValue)
                location = new Point(addr.Longitude.Value, addr.Latitude.Value) { SRID = 4326 };

            order.DeliveryAddress = new DeliveryAddress(Guid.NewGuid())
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

        var cartItemsForReservation = cart.Items
            .Select(i => (i.ProductId, i.Quantity))
            .ToList();

        db.Orders.Add(order);
        db.OrderItems.AddRange(items);
        db.OrderStatusHistory.Add(statusEntry);
        db.CartItems.RemoveRange(cart.Items);
        db.Carts.Remove(cart);

        await db.SaveChangesAsync(ct);

        var reserved = new List<(Guid ProductId, int Quantity)>(cartItemsForReservation.Count);
        try
        {
            foreach (var (productId, quantity) in cartItemsForReservation)
            {
                await catalogClient.ReserveStockAsync(productId, quantity, orderId, ct);
                reserved.Add((productId, quantity));
            }
        }
        catch
        {
            foreach (var (productId, quantity) in reserved)
            {
                try { await catalogClient.ReleaseStockAsync(productId, quantity, orderId, CancellationToken.None); }
                catch { /* best-effort: stock will reconcile via StockReservationFailedConsumer */ }
            }

            await transaction.RollbackAsync(ct);
            throw new ConflictException("Stock reservation failed. Please try again.");
        }

        await transaction.CommitAsync(ct);

        await eventBus.PublishAsync(new OrderPlacedEvent(
            orderId, request.UserId, sellerId, order.TotalAmount, now), ct);

        var created = await db.Orders
            .Include(o => o.Items)
            .Include(o => o.StatusHistory)
            .Include(o => o.Photos)
            .Include(o => o.DeliveryAddress)
            .Include(o => o.CourierAssignment)
            .Include(o => o.Dispute)
            .FirstAsync(o => o.Id == orderId, ct);

        return mapper.Map<OrderDto>(created);
    }
}
