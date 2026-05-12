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

public sealed class DisputeResolvedConsumer(
    NotificationDbContext db, IDistributedCache cache, IEmailService email,
    IOptions<FrontendOptions> frontend, INotificationPusher pusher)
    : IConsumer<DisputeResolvedEvent>
{
    public async Task Consume(ConsumeContext<DisputeResolvedEvent> ctx)
    {
        var msg = ctx.Message;
        var payload = JsonDocument.Parse(JsonSerializer.Serialize(new { orderId = msg.OrderId, disputeId = msg.DisputeId }));
        var now = msg.OccurredAt;

        var (buyerBody, sellerBody, outcomeText) = msg.Outcome switch
        {
            DisputeOutcome.BuyerFavored =>
                ("Спор решён в вашу пользу. Заказ отменён.", "Спор решён в пользу покупателя.", "в пользу покупателя"),
            DisputeOutcome.SellerFavored =>
                ("Спор решён в пользу продавца. Заказ засчитан как выполненный.", "Спор решён в вашу пользу. Заказ засчитан.", "в пользу продавца"),
            _ => ("Спор по заказу был разрешён.", "Спор по заказу был разрешён.", "завершён")
        };

        var buyerPrefs = await PreferenceHelper.GetAsync(msg.BuyerId, cache, db, ctx.CancellationToken);
        var sellerPrefs = await PreferenceHelper.GetAsync(msg.SellerId, cache, db, ctx.CancellationToken);

        NotificationEntity? buyerNotif = null;
        NotificationEntity? sellerNotif = null;

        if (buyerPrefs.OrderUpdates) { buyerNotif = Make(msg.BuyerId, buyerBody, payload, now); db.Notifications.Add(buyerNotif); }
        if (sellerPrefs.OrderUpdates) { sellerNotif = Make(msg.SellerId, sellerBody, payload, now); db.Notifications.Add(sellerNotif); }

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

        var orderUrl = frontend.Value.OrderUrl(msg.OrderId);
        if (buyerPrefs.CanEmailMandatory)
        {
            var (subject, body) = EmailTemplates.DisputeResolved(msg.OrderId, outcomeText, orderUrl);
            await email.SendAsync(buyerPrefs.Email!, subject, body, ctx.CancellationToken);
        }
        if (sellerPrefs.CanEmailMandatory)
        {
            var (subject, body) = EmailTemplates.DisputeResolved(msg.OrderId, outcomeText, orderUrl);
            await email.SendAsync(sellerPrefs.Email!, subject, body, ctx.CancellationToken);
        }
    }

    private static NotificationEntity Make(Guid userId, string body, JsonDocument payload, DateTimeOffset now) =>
        new(Guid.NewGuid())
        {
            UserId = userId,
            Type = NotificationType.DisputeResolved,
            Title = "Спор разрешён",
            Body = body,
            DeliveryChannel = DeliveryChannel.InApp,
            DeliveryStatus = DeliveryStatus.Sent,
            Payload = payload,
            SentAt = now,
            CreatedAt = now
        };
}
