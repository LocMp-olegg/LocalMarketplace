namespace LocMp.Notification.Api.Requests;

public sealed record UpdatePreferencesRequest(
    bool OrderUpdates,
    bool ReviewReplies,
    bool SystemAlerts,
    bool ChatMessages,
    bool EmailEnabled,
    bool EmailOrderUpdates,
    bool EmailReviewReplies,
    bool EmailChatMessages);
