using System.Text.Json;
using LocMp.Notification.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace LocMp.Notification.Infrastructure.Cache;

internal static class PreferenceHelper
{
    private static readonly DistributedCacheEntryOptions PrefsTtl =
        new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15) };

    public static async Task<CachedPreference> GetAsync(
        Guid userId, IDistributedCache cache, NotificationDbContext db, CancellationToken ct)
    {
        var key = NotificationCacheKeys.Prefs(userId);
        var raw = await cache.GetStringAsync(key, ct);
        if (raw is not null)
            return JsonSerializer.Deserialize<CachedPreference>(raw)!;

        var prefs = await db.UserNotificationPreferences
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        var result = prefs is null
            ? CachedPreference.Default
            : new CachedPreference(prefs.OrderUpdates, prefs.ReviewReplies, prefs.SystemAlerts);

        await cache.SetStringAsync(key, JsonSerializer.Serialize(result), PrefsTtl, ct);
        return result;
    }
}
