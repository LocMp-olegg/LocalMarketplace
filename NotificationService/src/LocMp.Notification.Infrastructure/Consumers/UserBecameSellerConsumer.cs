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

public sealed class UserBecameSellerConsumer(
    NotificationDbContext db, IDistributedCache cache, IEmailService email)
    : IConsumer<UserBecameSellerEvent>
{
    public async Task Consume(ConsumeContext<UserBecameSellerEvent> ctx)
    {
        var msg = ctx.Message;
        var prefs = await PreferenceHelper.GetAsync(msg.UserId, cache, db, ctx.CancellationToken);

        if (prefs.SystemAlerts)
        {
            db.Notifications.Add(new NotificationEntity(Guid.NewGuid())
            {
                UserId = msg.UserId,
                Type = NotificationType.SellerActivated,
                Title = "Аккаунт продавца активирован",
                Body = $"Добро пожаловать, {msg.DisplayName}! Ваш аккаунт продавца активирован — вы можете добавлять товары.",
                DeliveryChannel = DeliveryChannel.InApp,
                DeliveryStatus = DeliveryStatus.Sent,
                SentAt = msg.OccurredAt,
                CreatedAt = msg.OccurredAt
            });
            await db.SaveChangesAsync(ctx.CancellationToken);
            await cache.RemoveAsync(NotificationCacheKeys.UnreadCount(msg.UserId), ctx.CancellationToken);
        }

        if (prefs.CanEmailSystem)
        {
            var (subject, body) = EmailTemplates.SellerActivated(msg.DisplayName);
            await email.SendAsync(prefs.Email!, subject, body, ctx.CancellationToken);
        }
    }
}
