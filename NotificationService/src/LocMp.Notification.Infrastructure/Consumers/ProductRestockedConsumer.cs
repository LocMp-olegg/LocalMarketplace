using System.Text.Json;
using LocMp.Contracts.Catalog;
using LocMp.Notification.Domain.Enums;
using LocMp.Notification.Infrastructure.Cache;
using LocMp.Notification.Infrastructure.Persistence;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using NotificationEntity = LocMp.Notification.Domain.Entities.Notification;

namespace LocMp.Notification.Infrastructure.Consumers;

public sealed class ProductRestockedConsumer(NotificationDbContext db, IDistributedCache cache)
    : IConsumer<ProductRestockedEvent>
{
    public async Task Consume(ConsumeContext<ProductRestockedEvent> ctx)
    {
        var msg = ctx.Message;
        if (msg.FavoritedByUserIds.Count == 0) return;

        var payload = JsonDocument.Parse(JsonSerializer.Serialize(new { productId = msg.ProductId }));
        var now = msg.OccurredAt;
        var notifiedUserIds = new List<Guid>();

        foreach (var userId in msg.FavoritedByUserIds)
        {
            var prefs = await PreferenceHelper.GetAsync(userId, cache, db, ctx.CancellationToken);
            if (!prefs.SystemAlerts) continue;

            db.Notifications.Add(new NotificationEntity(Guid.NewGuid())
            {
                UserId = userId,
                Type = NotificationType.ProductRestocked,
                Title = "Товар снова в наличии",
                Body = $"Товар «{msg.ProductName}» из вашего избранного снова доступен для покупки.",
                DeliveryChannel = DeliveryChannel.InApp,
                DeliveryStatus = DeliveryStatus.Sent,
                Payload = payload,
                SentAt = now,
                CreatedAt = now
            });

            notifiedUserIds.Add(userId);
        }

        if (notifiedUserIds.Count == 0) return;

        await db.SaveChangesAsync(ctx.CancellationToken);

        foreach (var userId in notifiedUserIds)
            await cache.RemoveAsync(NotificationCacheKeys.UnreadCount(userId), ctx.CancellationToken);
    }
}
