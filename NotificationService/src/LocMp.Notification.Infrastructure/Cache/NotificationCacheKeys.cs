namespace LocMp.Notification.Infrastructure.Cache;

internal static class NotificationCacheKeys
{
    public static string Prefs(Guid userId) => $"notif:prefs:{userId}";
    public static string UnreadCount(Guid userId) => $"notif:unread:{userId}";
}
