using NotificationEntity = LocMp.Notification.Domain.Entities.Notification;

namespace LocMp.Notification.Domain;

public interface INotificationPusher
{
    Task PushAsync(Guid userId, NotificationPushDto notification, CancellationToken ct = default);
}

public sealed record NotificationPushDto(Guid Id, string Type, string Title, string Body, DateTimeOffset CreatedAt)
{
    public static NotificationPushDto From(NotificationEntity n) =>
        new(n.Id, n.Type.ToString(), n.Title, n.Body, n.CreatedAt);
}