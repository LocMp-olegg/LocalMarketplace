using LocMp.Contracts.Identity;
using LocMp.Notification.Domain;
using LocMp.Notification.Domain.Enums;
using LocMp.Notification.Infrastructure.Cache;
using LocMp.Notification.Infrastructure.Email;
using LocMp.Notification.Infrastructure.Persistence;
using LocMp.Notification.Infrastructure.Services;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using NotificationEntity = LocMp.Notification.Domain.Entities.Notification;

namespace LocMp.Notification.Infrastructure.Consumers;

public sealed class UserBlockedConsumer(
    NotificationDbContext db, IDistributedCache cache, IEmailService email,
    INotificationPusher pusher)
    : IConsumer<UserBlockedEvent>
{
    public async Task Consume(ConsumeContext<UserBlockedEvent> ctx)
    {
        var msg = ctx.Message;
        var prefs = await PreferenceHelper.GetAsync(msg.UserId, cache, db, ctx.CancellationToken);

        if (prefs.SystemAlerts)
        {
            var notif = new NotificationEntity(Guid.NewGuid())
            {
                UserId = msg.UserId,
                Type = NotificationType.AccountBlocked,
                Title = "Аккаунт заблокирован",
                Body = $"Ваш аккаунт заблокирован до {msg.BlockedUntil:dd.MM.yyyy HH:mm}.",
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

        if (prefs.CanEmailMandatory)
        {
            var (subject, body) = EmailTemplates.AccountBlocked(msg.BlockedUntil);
            await email.SendAsync(prefs.Email!, subject, body, ctx.CancellationToken);
        }
    }
}
