using LocMp.Contracts.Identity;
using LocMp.Notification.Infrastructure.Services;
using LocMp.Notification.Domain.Enums;
using LocMp.Notification.Infrastructure.Cache;
using LocMp.Notification.Infrastructure.Email;
using LocMp.Notification.Infrastructure.Persistence;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using NotificationEntity = LocMp.Notification.Domain.Entities.Notification;

namespace LocMp.Notification.Infrastructure.Consumers;

public sealed class UserUnblockedConsumer(
    NotificationDbContext db, IDistributedCache cache, IEmailService email)
    : IConsumer<UserUnblockedEvent>
{
    public async Task Consume(ConsumeContext<UserUnblockedEvent> ctx)
    {
        var msg = ctx.Message;
        var prefs = await PreferenceHelper.GetAsync(msg.UserId, cache, db, ctx.CancellationToken);

        if (prefs.SystemAlerts)
        {
            db.Notifications.Add(new NotificationEntity(Guid.NewGuid())
            {
                UserId = msg.UserId,
                Type = NotificationType.AccountUnblocked,
                Title = "Аккаунт разблокирован",
                Body = "Ваш аккаунт был разблокирован. Вы снова можете пользоваться платформой.",
                DeliveryChannel = DeliveryChannel.InApp,
                DeliveryStatus = DeliveryStatus.Sent,
                SentAt = msg.OccurredAt,
                CreatedAt = msg.OccurredAt
            });
            await db.SaveChangesAsync(ctx.CancellationToken);
            await cache.RemoveAsync(NotificationCacheKeys.UnreadCount(msg.UserId), ctx.CancellationToken);
        }

        // Mandatory email
        if (prefs.CanEmailMandatory)
        {
            var (subject, body) = EmailTemplates.AccountUnblocked();
            await email.SendAsync(prefs.Email!, subject, body, ctx.CancellationToken);
        }
    }
}
