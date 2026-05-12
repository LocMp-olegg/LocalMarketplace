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

public sealed class DisputeOpenedConsumer(
    NotificationDbContext db, IDistributedCache cache, IEmailService email,
    IOptions<FrontendOptions> frontend, INotificationPusher pusher)
    : IConsumer<DisputeOpenedEvent>
{
    public async Task Consume(ConsumeContext<DisputeOpenedEvent> ctx)
    {
        var msg = ctx.Message;
        var payload = JsonDocument.Parse(JsonSerializer.Serialize(new { orderId = msg.OrderId, disputeId = msg.DisputeId }));
        var now = msg.OccurredAt;

        var buyerPrefs = await PreferenceHelper.GetAsync(msg.BuyerId, cache, db, ctx.CancellationToken);
        var sellerPrefs = await PreferenceHelper.GetAsync(msg.SellerId, cache, db, ctx.CancellationToken);

        NotificationEntity? buyerNotif = null;
        NotificationEntity? sellerNotif = null;

        if (buyerPrefs.OrderUpdates)
        {
            buyerNotif = Make(msg.BuyerId,
                "По заказу открыт спор. Наши администраторы рассмотрят ситуацию и примут решение.", payload, now);
            db.Notifications.Add(buyerNotif);
        }

        if (sellerPrefs.OrderUpdates)
        {
            sellerNotif = Make(msg.SellerId,
                "Покупатель открыл спор по заказу. Наши администраторы рассмотрят ситуацию.", payload, now);
            db.Notifications.Add(sellerNotif);
        }

        if (buyerNotif is not null || sellerNotif is not null)
        {
            await db.SaveChangesAsync(ctx.CancellationToken);
            if (buyerNotif is not null)
            {
                await cache.RemoveAsync(NotificationCacheKeys.UnreadCount(msg.BuyerId), ctx.CancellationToken);
                await pusher.PushAsync(msg.BuyerId, NotificationPushDto.From(buyerNotif), ctx.CancellationToken);
            }
            if (sellerNotif is not null)
            {
                await cache.RemoveAsync(NotificationCacheKeys.UnreadCount(msg.SellerId), ctx.CancellationToken);
                await pusher.PushAsync(msg.SellerId, NotificationPushDto.From(sellerNotif), ctx.CancellationToken);
            }
        }

        var (subject, body) = EmailTemplates.DisputeOpened(
            msg.OrderId, frontend.Value.OrderUrl(msg.OrderId));
        if (buyerPrefs.CanEmailMandatory)
            await email.SendAsync(buyerPrefs.Email!, subject, body, ctx.CancellationToken);
        if (sellerPrefs.CanEmailMandatory)
            await email.SendAsync(sellerPrefs.Email!, subject, body, ctx.CancellationToken);
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
