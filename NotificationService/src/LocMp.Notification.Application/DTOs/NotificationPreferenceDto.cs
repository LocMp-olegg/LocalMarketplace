namespace LocMp.Notification.Application.DTOs;

public sealed record NotificationPreferenceDto(
    bool OrderUpdates,
    bool ReviewReplies,
    bool SystemAlerts,
    bool ChatMessages,
    bool EmailEnabled,
    bool EmailOrderUpdates,
    bool EmailReviewReplies,
    bool EmailChatMessages);
