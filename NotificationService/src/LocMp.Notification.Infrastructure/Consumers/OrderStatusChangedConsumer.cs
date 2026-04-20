using System.Text.Json;
using LocMp.Contracts.Orders;
using LocMp.Notification.Domain.Enums;
using LocMp.Notification.Infrastructure.Cache;
using LocMp.Notification.Infrastructure.Persistence;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using NotificationEntity = LocMp.Notification.Domain.Entities.Notification;

namespace LocMp.Notification.Infrastructure.Consumers;

public sealed class OrderStatusChangedConsumer(NotificationDbContext db, IDistributedCache cache)
    : IConsumer<OrderStatusChangedEvent>
{
    public async Task Consume(ConsumeContext<OrderStatusChangedEvent> ctx)
    {
        var msg = ctx.Message;
        var payload = JsonDocument.Parse(JsonSerializer.Serialize(new { orderId = msg.OrderId }));
        var now = msg.OccurredAt;

        NotificationEntity? buyerNotif = null;
        NotificationEntity? sellerNotif = null;

        switch (msg.ToStatus)
        {
            case "Confirmed":
            {
                var prefs = await PreferenceHelper.GetAsync(msg.BuyerId, cache, db, ctx.CancellationToken);
                if (prefs.OrderUpdates)
                    buyerNotif = Make(msg.BuyerId, NotificationType.OrderConfirmed,
                        "Заказ подтверждён", "Продавец подтвердил ваш заказ.", payload, now);
                break;
            }
            case "ReadyForPickup":
            {
                var prefs = await PreferenceHelper.GetAsync(msg.BuyerId, cache, db, ctx.CancellationToken);
                if (prefs.OrderUpdates)
                    buyerNotif = Make(msg.BuyerId, NotificationType.OrderReadyForPickup,
                        "Заказ готов", "Ваш заказ готов к самовывозу.", payload, now);
                break;
            }
            case "InDelivery":
            {
                var prefs = await PreferenceHelper.GetAsync(msg.BuyerId, cache, db, ctx.CancellationToken);
                if (prefs.OrderUpdates)
                    buyerNotif = Make(msg.BuyerId, NotificationType.OrderInDelivery,
                        "Заказ передан курьеру", "Ваш заказ передан курьеру-соседу и скоро будет доставлен.", payload, now);
                break;
            }
            case "Cancelled":
            {
                var buyerPrefs = await PreferenceHelper.GetAsync(msg.BuyerId, cache, db, ctx.CancellationToken);
                if (buyerPrefs.OrderUpdates)
                    buyerNotif = Make(msg.BuyerId, NotificationType.OrderCancelled,
                        "Заказ отменён", "Ваш заказ был отменён.", payload, now);

                var sellerPrefs = await PreferenceHelper.GetAsync(msg.SellerId, cache, db, ctx.CancellationToken);
                if (sellerPrefs.OrderUpdates)
                    sellerNotif = Make(msg.SellerId, NotificationType.OrderCancelled,
                        "Заказ отменён", "Заказ был отменён покупателем.", payload, now);
                break;
            }
        }

        if (buyerNotif is not null) db.Notifications.Add(buyerNotif);
        if (sellerNotif is not null) db.Notifications.Add(sellerNotif);

        if (buyerNotif is null && sellerNotif is null) return;

        await db.SaveChangesAsync(ctx.CancellationToken);

        if (buyerNotif is not null)
            await cache.RemoveAsync(NotificationCacheKeys.UnreadCount(msg.BuyerId), ctx.CancellationToken);
        if (sellerNotif is not null)
            await cache.RemoveAsync(NotificationCacheKeys.UnreadCount(msg.SellerId), ctx.CancellationToken);
    }

    private static NotificationEntity Make(
        Guid userId, NotificationType type, string title, string body,
        JsonDocument payload, DateTimeOffset now) =>
        new(Guid.NewGuid())
        {
            UserId = userId,
            Type = type,
            Title = title,
            Body = body,
            DeliveryChannel = DeliveryChannel.InApp,
            DeliveryStatus = DeliveryStatus.Sent,
            Payload = payload,
            SentAt = now,
            CreatedAt = now
        };
}
