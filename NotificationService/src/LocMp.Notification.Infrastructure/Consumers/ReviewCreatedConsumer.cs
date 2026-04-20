using System.Text.Json;
using LocMp.Contracts.Review;
using LocMp.Notification.Infrastructure.Services;
using LocMp.Notification.Domain.Enums;
using LocMp.Notification.Infrastructure.Cache;
using LocMp.Notification.Infrastructure.Email;
using LocMp.Notification.Infrastructure.Persistence;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using NotificationEntity = LocMp.Notification.Domain.Entities.Notification;

namespace LocMp.Notification.Infrastructure.Consumers;

public sealed class ReviewCreatedConsumer(
    NotificationDbContext db, IDistributedCache cache, IEmailService email)
    : IConsumer<ReviewCreatedEvent>
{
    public async Task Consume(ConsumeContext<ReviewCreatedEvent> ctx)
    {
        var msg = ctx.Message;
        var prefs = await PreferenceHelper.GetAsync(msg.SellerId, cache, db, ctx.CancellationToken);

        if (prefs.ReviewReplies)
        {
            var payload = JsonDocument.Parse(JsonSerializer.Serialize(new { reviewId = msg.ReviewId, subjectId = msg.SubjectId }));
            var stars = new string('★', msg.Rating) + new string('☆', 5 - msg.Rating);
            db.Notifications.Add(new NotificationEntity(Guid.NewGuid())
            {
                UserId = msg.SellerId,
                Type = NotificationType.ReviewReceived,
                Title = "Новый отзыв",
                Body = $"Вы получили новый отзыв: {stars} ({msg.Rating}/5).",
                DeliveryChannel = DeliveryChannel.InApp,
                DeliveryStatus = DeliveryStatus.Sent,
                Payload = payload,
                SentAt = msg.OccurredAt,
                CreatedAt = msg.OccurredAt
            });
            await db.SaveChangesAsync(ctx.CancellationToken);
            await cache.RemoveAsync(NotificationCacheKeys.UnreadCount(msg.SellerId), ctx.CancellationToken);
        }

        if (prefs.CanEmailReview)
        {
            var (subject, body) = EmailTemplates.ReviewCreated(msg.Rating);
            await email.SendAsync(prefs.Email!, subject, body, ctx.CancellationToken);
        }
    }
}
