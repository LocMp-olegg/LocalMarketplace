namespace LocMp.Notification.Api.Requests;

public sealed record UpdatePreferencesRequest(bool OrderUpdates, bool ReviewReplies, bool SystemAlerts);