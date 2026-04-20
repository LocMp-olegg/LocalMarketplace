using System.Text.Json;
using LocMp.Contracts.Catalog;
using LocMp.Notification.Domain.Enums;
using LocMp.Notification.Infrastructure.Cache;
using LocMp.Notification.Infrastructure.Persistence;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using NotificationEntity = LocMp.Notification.Domain.Entities.Notification;

namespace LocMp.Notification.Infrastructure.Consumers;

public sealed class StockDepletedConsumer(NotificationDbContext db, IDistributedCache cache)
    : IConsumer<StockDepletedEvent>
{
    public async Task Consume(ConsumeContext<StockDepletedEvent> ctx)
    {
        var msg = ctx.Message;
        var prefs = await PreferenceHelper.GetAsync(msg.SellerId, cache, db, ctx.CancellationToken);
        if (!prefs.SystemAlerts) return;

        var payload = JsonDocument.Parse(JsonSerializer.Serialize(new { productId = msg.ProductId }));

        db.Notifications.Add(new NotificationEntity(Guid.NewGuid())
        {
            UserId = msg.SellerId,
            Type = NotificationType.StockDepleted,
            Title = "Товар закончился",
            Body = $"Товар «{msg.ProductName}» закончился. Пополните остатки, чтобы не терять покупателей.",
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
