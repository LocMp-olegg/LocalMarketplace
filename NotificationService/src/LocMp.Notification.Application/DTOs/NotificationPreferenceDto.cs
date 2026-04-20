namespace LocMp.Notification.Application.DTOs;

public sealed record NotificationPreferenceDto(
    bool OrderUpdates,
    bool ReviewReplies,
    bool SystemAlerts);
