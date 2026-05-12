using System.Text.Json;
using LocMp.Contracts.Catalog;
using LocMp.Notification.Domain;
using LocMp.Notification.Domain.Enums;
using LocMp.Notification.Infrastructure.Cache;
using LocMp.Notification.Infrastructure.Email;
using LocMp.Notification.Infrastructure.Options;
using LocMp.Notification.Infrastructure.Persistence;
using LocMp.Notification.Infrastructure.Services;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using NotificationEntity = LocMp.Notification.Domain.Entities.Notification;

namespace LocMp.Notification.Infrastructure.Consumers;

public sealed class ProductRestockedConsumer(
    NotificationDbContext db, IDistributedCache cache, IEmailService email,
    IOptions<FrontendOptions> frontend, INotificationPusher pusher)
    : IConsumer<ProductRestockedEvent>
{
    public async Task Consume(ConsumeContext<ProductRestockedEvent> ctx)
    {
        var msg = ctx.Message;
        if (msg.FavoritedByUserIds.Count == 0) return;

        var payload = JsonDocument.Parse(JsonSerializer.Serialize(new { productId = msg.ProductId }));
        var now = msg.OccurredAt;
        var toNotify = new List<(Guid UserId, NotificationEntity Notif)>();

        foreach (var userId in msg.FavoritedByUserIds)
        {
            var prefs = await PreferenceHelper.GetAsync(userId, cache, db, ctx.CancellationToken);

            if (prefs.SystemAlerts)
            {
                var notif = new NotificationEntity(Guid.NewGuid())
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
                };
                db.Notifications.Add(notif);
                toNotify.Add((userId, notif));
            }

            if (prefs.CanEmailSystem)
            {
                var (subject, body) = EmailTemplates.ProductRestocked(
                    msg.ProductName, frontend.Value.ProductUrl(msg.ProductId));
                await email.SendAsync(prefs.Email!, subject, body, ctx.CancellationToken);
            }
        }

        if (toNotify.Count == 0) return;

        await db.SaveChangesAsync(ctx.CancellationToken);
        foreach (var (userId, notif) in toNotify)
        {
            await cache.RemoveAsync(NotificationCacheKeys.UnreadCount(userId), ctx.CancellationToken);
            await pusher.PushAsync(userId, NotificationPushDto.From(notif), ctx.CancellationToken);
        }
    }
}
