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

public sealed class StockDepletedConsumer(
    NotificationDbContext db, IDistributedCache cache, IEmailService email,
    IOptions<FrontendOptions> frontend, INotificationPusher pusher)
    : IConsumer<StockDepletedEvent>
{
    public async Task Consume(ConsumeContext<StockDepletedEvent> ctx)
    {
        var msg = ctx.Message;
        var prefs = await PreferenceHelper.GetAsync(msg.SellerId, cache, db, ctx.CancellationToken);

        if (prefs.SystemAlerts)
        {
            var payload = JsonDocument.Parse(JsonSerializer.Serialize(new { productId = msg.ProductId }));
            var notif = new NotificationEntity(Guid.NewGuid())
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
            };
            db.Notifications.Add(notif);
            await db.SaveChangesAsync(ctx.CancellationToken);
            await cache.RemoveAsync(NotificationCacheKeys.UnreadCount(msg.SellerId), ctx.CancellationToken);
            await pusher.PushAsync(msg.SellerId, NotificationPushDto.From(notif), ctx.CancellationToken);
        }

        if (prefs.CanEmailSystem)
        {
            var (subject, body) = EmailTemplates.StockDepleted(
                msg.ProductName, frontend.Value.ProductEditUrl(msg.ProductId));
            await email.SendAsync(prefs.Email!, subject, body, ctx.CancellationToken);
        }
    }
}
