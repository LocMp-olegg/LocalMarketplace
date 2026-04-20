namespace LocMp.Notification.Infrastructure.Cache;

internal sealed record CachedPreference(bool OrderUpdates, bool ReviewReplies, bool SystemAlerts)
{
    public static readonly CachedPreference Default = new(true, true, true);
}
