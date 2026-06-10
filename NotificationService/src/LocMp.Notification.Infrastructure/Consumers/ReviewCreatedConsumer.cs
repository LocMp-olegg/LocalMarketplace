using System.Text.Json;
using LocMp.Contracts.Review;
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

public sealed class ReviewCreatedConsumer(
    NotificationDbContext db,
    IDistributedCache cache,
    IEmailService email,
    IOptions<FrontendOptions> frontend,
    INotificationPusher pusher)
    : IConsumer<ReviewCreatedEvent>
{
    public async Task Consume(ConsumeContext<ReviewCreatedEvent> ctx)
    {
        var msg = ctx.Message;
        var recipientId = msg.SubjectType == "Courier" ? msg.SubjectId : msg.SellerId;
        var prefs = await PreferenceHelper.GetAsync(recipientId, cache, db, ctx.CancellationToken);

        if (prefs.ReviewReplies)
        {
            var payload = JsonDocument.Parse(JsonSerializer.Serialize(new
                { reviewId = msg.ReviewId, subjectId = msg.SubjectId, subjectType = msg.SubjectType }));
            var stars = new string('★', msg.Rating) + new string('☆', 5 - msg.Rating);
            var notif = new NotificationEntity(Guid.NewGuid())
            {
                UserId = recipientId,
                Type = NotificationType.ReviewReceived,
                Title = "Новый отзыв",
                Body = $"Вы получили новый отзыв: {stars} ({msg.Rating}/5).",
                DeliveryChannel = DeliveryChannel.InApp,
                DeliveryStatus = DeliveryStatus.Sent,
                Payload = payload,
                SentAt = msg.OccurredAt,
                CreatedAt = msg.OccurredAt
            };
            db.Notifications.Add(notif);
            await db.SaveChangesAsync(ctx.CancellationToken);
            await cache.RemoveAsync(NotificationCacheKeys.UnreadCount(recipientId), ctx.CancellationToken);
            await pusher.PushAsync(recipientId, NotificationPushDto.From(notif), ctx.CancellationToken);
        }

        if (prefs.CanEmailReview)
        {
            var reviewUrl = frontend.Value.ReviewUrl(msg.ReviewId);
            var productUrl = msg.SubjectType == "Product"
                ? frontend.Value.ProductUrl(msg.SubjectId)
                : null;
            var (subject, body) = EmailTemplates.ReviewCreated(
                msg.Rating, msg.SubjectType, reviewUrl, msg.SubjectName, productUrl);
            await email.SendAsync(prefs.Email!, subject, body, ctx.CancellationToken);
        }
    }
}
