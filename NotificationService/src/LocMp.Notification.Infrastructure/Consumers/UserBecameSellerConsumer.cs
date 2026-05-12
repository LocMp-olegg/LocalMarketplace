using LocMp.Contracts.Identity;
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

public sealed class UserBecameSellerConsumer(
    NotificationDbContext db, IDistributedCache cache, IEmailService email,
    IOptions<FrontendOptions> frontend, INotificationPusher pusher)
    : IConsumer<UserBecameSellerEvent>
{
    public async Task Consume(ConsumeContext<UserBecameSellerEvent> ctx)
    {
        var msg = ctx.Message;
        var prefs = await PreferenceHelper.GetAsync(msg.UserId, cache, db, ctx.CancellationToken);

        if (prefs.SystemAlerts)
        {
            var notif = new NotificationEntity(Guid.NewGuid())
            {
                UserId = msg.UserId,
                Type = NotificationType.SellerActivated,
                Title = "Аккаунт продавца активирован",
                Body = $"Добро пожаловать, {msg.DisplayName}! Ваш аккаунт продавца активирован — вы можете добавлять товары.",
                DeliveryChannel = DeliveryChannel.InApp,
                DeliveryStatus = DeliveryStatus.Sent,
                SentAt = msg.OccurredAt,
                CreatedAt = msg.OccurredAt
            };
            db.Notifications.Add(notif);
            await db.SaveChangesAsync(ctx.CancellationToken);
            await cache.RemoveAsync(NotificationCacheKeys.UnreadCount(msg.UserId), ctx.CancellationToken);
            await pusher.PushAsync(msg.UserId, NotificationPushDto.From(notif), ctx.CancellationToken);
        }

        if (prefs.CanEmailSystem)
        {
            var (subject, body) = EmailTemplates.SellerActivated(
                msg.DisplayName, frontend.Value.SellerShopsUrl());
            await email.SendAsync(prefs.Email!, subject, body, ctx.CancellationToken);
        }
    }
}
