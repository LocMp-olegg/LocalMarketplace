using System.Text.Json;
using LocMp.Contracts.Orders;
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

public sealed class OrderPlacedConsumer(
    NotificationDbContext db, IDistributedCache cache, IEmailService email,
    IOptions<FrontendOptions> frontend, INotificationPusher pusher)
    : IConsumer<OrderPlacedEvent>
{
    public async Task Consume(ConsumeContext<OrderPlacedEvent> ctx)
    {
        var msg = ctx.Message;
        var prefs = await PreferenceHelper.GetAsync(msg.SellerId, cache, db, ctx.CancellationToken);

        if (prefs.OrderUpdates)
        {
            var payload = JsonDocument.Parse(JsonSerializer.Serialize(new { orderId = msg.OrderId }));
            var notif = new NotificationEntity(Guid.NewGuid())
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
            };
            db.Notifications.Add(notif);
            await db.SaveChangesAsync(ctx.CancellationToken);
            await cache.RemoveAsync(NotificationCacheKeys.UnreadCount(msg.SellerId), ctx.CancellationToken);
            await pusher.PushAsync(msg.SellerId, NotificationPushDto.From(notif), ctx.CancellationToken);
        }

        if (prefs.CanEmailOrder)
        {
            var (subject, body) = EmailTemplates.OrderPlaced(
                msg.TotalAmount, msg.OrderId, frontend.Value.SellerOrdersUrl());
            await email.SendAsync(prefs.Email!, subject, body, ctx.CancellationToken);
        }
    }
}
