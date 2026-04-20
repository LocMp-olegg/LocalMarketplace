using System.Text.Json;
using LocMp.Contracts.Orders;
using LocMp.Notification.Infrastructure.Services;
using LocMp.Notification.Domain.Enums;
using LocMp.Notification.Infrastructure.Cache;
using LocMp.Notification.Infrastructure.Email;
using LocMp.Notification.Infrastructure.Persistence;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using NotificationEntity = LocMp.Notification.Domain.Entities.Notification;

namespace LocMp.Notification.Infrastructure.Consumers;

public sealed class OrderCompletedConsumer(
    NotificationDbContext db,
    IDistributedCache cache,
    IEmailService email)
    : IConsumer<OrderCompletedEvent>
{
    public async Task Consume(ConsumeContext<OrderCompletedEvent> ctx)
    {
        var msg = ctx.Message;
        var prefs = await PreferenceHelper.GetAsync(msg.BuyerId, cache, db, ctx.CancellationToken);

        if (prefs.OrderUpdates)
        {
            var payload = JsonDocument.Parse(JsonSerializer.Serialize(new { orderId = msg.OrderId }));
            db.Notifications.Add(new NotificationEntity(Guid.NewGuid())
            {
                UserId = msg.BuyerId,
                Type = NotificationType.OrderCompleted,
                Title = "Заказ получен",
                Body = "Заказ успешно завершён. Оставьте отзыв о продавце и товарах!",
                DeliveryChannel = DeliveryChannel.InApp,
                DeliveryStatus = DeliveryStatus.Sent,
                Payload = payload,
                SentAt = msg.OccurredAt,
                CreatedAt = msg.OccurredAt
            });
            await db.SaveChangesAsync(ctx.CancellationToken);
            await cache.RemoveAsync(NotificationCacheKeys.UnreadCount(msg.BuyerId), ctx.CancellationToken);
        }

        if (prefs.CanEmailOrder)
        {
            var (subject, body) = EmailTemplates.OrderCompleted(msg.OrderId);
            await email.SendAsync(prefs.Email!, subject, body, ctx.CancellationToken);
        }
    }
}