using LocMp.Notification.Domain.Enums;

namespace LocMp.Notification.Application.DTOs;

public sealed record NotificationDto(
    Guid Id,
    NotificationType Type,
    string Title,
    string Body,
    bool IsRead,
    DateTimeOffset? ReadAt,
    DateTimeOffset CreatedAt);
