using System.Text.Json;
using LocMp.Contracts.Orders;
using LocMp.Notification.Domain.Enums;
using LocMp.Notification.Infrastructure.Cache;
using LocMp.Notification.Infrastructure.Persistence;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using NotificationEntity = LocMp.Notification.Domain.Entities.Notification;

namespace LocMp.Notification.Infrastructure.Consumers;

public sealed class DisputeOpenedConsumer(NotificationDbContext db, IDistributedCache cache)
    : IConsumer<DisputeOpenedEvent>
{
    public async Task Consume(ConsumeContext<DisputeOpenedEvent> ctx)
    {
        var msg = ctx.Message;
        var payload = JsonDocument.Parse(JsonSerializer.Serialize(new { orderId = msg.OrderId, disputeId = msg.DisputeId }));
        var now = msg.OccurredAt;

        var buyerPrefs = await PreferenceHelper.GetAsync(msg.BuyerId, cache, db, ctx.CancellationToken);
        var sellerPrefs = await PreferenceHelper.GetAsync(msg.SellerId, cache, db, ctx.CancellationToken);

        if (buyerPrefs.OrderUpdates)
            db.Notifications.Add(Make(msg.BuyerId,
                "По заказу открыт спор. Наши администраторы рассмотрят ситуацию и примут решение.", payload, now));

        if (sellerPrefs.OrderUpdates)
            db.Notifications.Add(Make(msg.SellerId,
                "Покупатель открыл спор по заказу. Наши администраторы рассмотрят ситуацию.", payload, now));

        if (!buyerPrefs.OrderUpdates && !sellerPrefs.OrderUpdates) return;

        await db.SaveChangesAsync(ctx.CancellationToken);

        if (buyerPrefs.OrderUpdates)
            await cache.RemoveAsync(NotificationCacheKeys.UnreadCount(msg.BuyerId), ctx.CancellationToken);
        if (sellerPrefs.OrderUpdates)
            await cache.RemoveAsync(NotificationCacheKeys.UnreadCount(msg.SellerId), ctx.CancellationToken);
    }

    private static NotificationEntity Make(Guid userId, string body, JsonDocument payload, DateTimeOffset now) =>
        new(Guid.NewGuid())
        {
            UserId = userId,
            Type = NotificationType.DisputeOpened,
            Title = "Открыт спор",
            Body = body,
            DeliveryChannel = DeliveryChannel.InApp,
            DeliveryStatus = DeliveryStatus.Sent,
            Payload = payload,
            SentAt = now,
            CreatedAt = now
        };
}
