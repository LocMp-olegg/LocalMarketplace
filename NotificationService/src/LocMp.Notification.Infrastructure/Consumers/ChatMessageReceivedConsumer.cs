using System.Text.Json;
using LocMp.Contracts.Chat;
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

public sealed class ChatMessageReceivedConsumer(
    NotificationDbContext db,
    IDistributedCache cache,
    IEmailService email,
    IOptions<FrontendOptions> frontend,
    INotificationPusher pusher)
    : IConsumer<ChatMessageSentEvent>
{
    public async Task Consume(ConsumeContext<ChatMessageSentEvent> ctx)
    {
        var msg = ctx.Message;

        var (title, inAppBody) = BuildText(msg);

        foreach (var recipientId in msg.RecipientIds)
        {
            var prefs = await PreferenceHelper.GetAsync(recipientId, cache, db, ctx.CancellationToken);

            if (prefs.ChatMessages)
            {
                var payload = JsonDocument.Parse(JsonSerializer.Serialize(new
                {
                    chatId = msg.ChatId,
                    messageId = msg.MessageId,
                    senderId = msg.SenderId,
                }));

                var notif = new NotificationEntity(Guid.NewGuid())
                {
                    UserId = recipientId,
                    Type = NotificationType.NewMessage,
                    Title = title,
                    Body = inAppBody,
                    DeliveryChannel = DeliveryChannel.InApp,
                    DeliveryStatus = DeliveryStatus.Sent,
                    Payload = payload,
                    SentAt = msg.OccurredAt,
                    CreatedAt = msg.OccurredAt,
                };
                db.Notifications.Add(notif);
                await db.SaveChangesAsync(ctx.CancellationToken);
                await cache.RemoveAsync(NotificationCacheKeys.UnreadCount(recipientId), ctx.CancellationToken);
                await pusher.PushAsync(recipientId, NotificationPushDto.From(notif), ctx.CancellationToken);
            }

            if (prefs.CanEmailChat)
            {
                var (subject, body) = EmailTemplates.ChatMessage(
                    msg.SenderName, inAppBody, frontend.Value.ChatUrl(msg.ChatId));
                await email.SendAsync(prefs.Email!, subject, body, ctx.CancellationToken);
            }
        }
    }

    private static (string Title, string Body) BuildText(ChatMessageSentEvent msg) =>
        msg.ChatType switch
        {
            "Support" => (
                "Техподдержка",
                $"{msg.SenderName} написал в техподдержку."),
            "Shop" when msg.SubjectName is { } shop => (
                "Сообщение в магазин",
                $"{msg.SenderName} написал в магазин «{shop}»."),
            "Shop" => (
                "Сообщение в магазин",
                $"{msg.SenderName} написал в ваш магазин."),
            "Order" => (
                "Сообщение по заказу",
                $"{msg.SenderName} написал сообщение по заказу."),
            _ => (
                "Новое сообщение",
                $"{msg.SenderName} написал вам сообщение.")
        };
}