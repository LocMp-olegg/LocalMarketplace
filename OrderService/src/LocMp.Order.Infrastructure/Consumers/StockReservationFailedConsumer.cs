using LocMp.Contracts.Catalog;
using LocMp.Contracts.Orders;
using LocMp.Order.Domain.Entities;
using LocMp.Order.Domain.Enums;
using LocMp.Order.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LocMp.Order.Infrastructure.Consumers;

/// <summary>
/// Handles async stock reservation failures from CatalogService.
/// Relevant when switching from synchronous HTTP reservation to event-driven flow.
/// Currently the checkout uses synchronous HTTP, so this consumer acts as a safety net.
/// </summary>
public sealed class StockReservationFailedConsumer(
    OrderDbContext db,
    ILogger<StockReservationFailedConsumer> logger)
    : IConsumer<StockReservationFailedEvent>
{
    public async Task Consume(ConsumeContext<StockReservationFailedEvent> context)
    {
        var msg = context.Message;
        var ct = context.CancellationToken;

        var order = await db.Orders
            .FirstOrDefaultAsync(o => o.Id == msg.OrderId && o.Status == OrderStatus.Pending, ct);

        if (order is null)
        {
            logger.LogWarning(
                "StockReservationFailed for OrderId {OrderId}, but order not found or not Pending. Ignoring.",
                msg.OrderId);
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var prev = order.Status;
        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = now;

        db.OrderStatusHistory.Add(new OrderStatusHistory(Guid.NewGuid())
        {
            OrderId = order.Id,
            FromStatus = prev,
            ToStatus = OrderStatus.Cancelled,
            Comment = $"Stock reservation failed: {msg.Reason}",
            ChangedById = order.BuyerId,
            ChangedAt = now
        });

        await db.SaveChangesAsync(ct);

        await context.Publish(new OrderStatusChangedEvent(
            order.Id, order.BuyerId, order.SellerId,
            prev.ToString(), OrderStatus.Cancelled.ToString(), now));

        logger.LogInformation(
            "Order {OrderId} cancelled due to stock reservation failure for product {ProductId}: {Reason}",
            msg.OrderId, msg.ProductId, msg.Reason);
    }
}
