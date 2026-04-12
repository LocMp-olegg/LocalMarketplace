using LocMp.Contracts.Identity;
using LocMp.Contracts.Orders;
using LocMp.Order.Domain.Entities;
using LocMp.Order.Domain.Enums;
using LocMp.Order.Infrastructure.Clients;
using LocMp.Order.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LocMp.Order.Infrastructure.Consumers;

public sealed class UserBlockedConsumer(
    OrderDbContext db,
    CatalogServiceClient catalogClient,
    ILogger<UserBlockedConsumer> logger)
    : IConsumer<UserBlockedEvent>
{
    private static readonly HashSet<OrderStatus> CancellableStatuses =
    [
        OrderStatus.Pending,
        OrderStatus.Confirmed
    ];

    public async Task Consume(ConsumeContext<UserBlockedEvent> context)
    {
        var userId = context.Message.UserId;
        var ct = context.CancellationToken;

        var orders = await db.Orders
            .Include(o => o.Items)
            .Where(o =>
                (o.BuyerId == userId || o.SellerId == userId) &&
                CancellableStatuses.Contains(o.Status))
            .ToListAsync(ct);

        if (orders.Count == 0) return;

        var now = DateTimeOffset.UtcNow;

        foreach (var order in orders)
        {
            var prev = order.Status;
            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = now;

            db.OrderStatusHistory.Add(new OrderStatusHistory(Guid.NewGuid())
            {
                OrderId = order.Id,
                FromStatus = prev,
                ToStatus = OrderStatus.Cancelled,
                Comment = "Cancelled due to user block",
                ChangedById = userId,
                ChangedAt = now
            });

            foreach (var item in order.Items)
            {
                try { await catalogClient.ReleaseStockAsync(item.ProductId, item.Quantity, order.Id, ct); }
                catch (Exception ex)
                {
                    logger.LogWarning(ex,
                        "Failed to release stock for product {ProductId} on order {OrderId} during user block",
                        item.ProductId, order.Id);
                }
            }

            await context.Publish(new OrderStatusChangedEvent(
                order.Id, order.BuyerId, order.SellerId,
                prev.ToString(), OrderStatus.Cancelled.ToString(), now));
        }

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Cancelled {Count} orders for blocked user {UserId}", orders.Count, userId);
    }
}
