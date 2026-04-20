using System.Text.Json;
using LocMp.Contracts.Orders;
using LocMp.Notification.Domain.Enums;
using LocMp.Notification.Infrastructure.Cache;
using LocMp.Notification.Infrastructure.Persistence;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using NotificationEntity = LocMp.Notification.Domain.Entities.Notification;

namespace LocMp.Notification.Infrastructure.Consumers;

public sealed class OrderPlacedConsumer(NotificationDbContext db, IDistributedCache cache)
    : IConsumer<OrderPlacedEvent>
{
    public async Task Consume(ConsumeContext<OrderPlacedEvent> ctx)
    {
        var msg = ctx.Message;
        var prefs = await PreferenceHelper.GetAsync(msg.SellerId, cache, db, ctx.CancellationToken);
        if (!prefs.OrderUpdates) return;

        var payload = JsonDocument.Parse(JsonSerializer.Serialize(new { orderId = msg.OrderId }));

        db.Notifications.Add(new NotificationEntity(Guid.NewGuid())
        {
            UserId = msg.SellerId,
            Type = NotificationType.OrderPlaced,
            Title = "Новый заказ",
            Body = $"Новый заказ на сумму {msg.TotalAmount:N2} ₽. Подтвердите его в ближайшее время.",
            DeliveryChannel = DeliveryChannel.InApp,
            DeliveryStatus = DeliveryStatus.Sent,
            Payload = payload,
            SentAt = msg.OccurredAt,
            CreatedAt = msg.OccurredAt
        });

        await db.SaveChangesAsync(ctx.CancellationToken);
        await cache.RemoveAsync(NotificationCacheKeys.UnreadCount(msg.SellerId), ctx.CancellationToken);
    }
}
