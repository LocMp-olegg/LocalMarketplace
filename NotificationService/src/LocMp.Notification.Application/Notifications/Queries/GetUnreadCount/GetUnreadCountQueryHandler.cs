using LocMp.Notification.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace LocMp.Notification.Application.Notifications.Queries.GetUnreadCount;

public sealed class GetUnreadCountQueryHandler(NotificationDbContext db, IDistributedCache cache)
    : IRequestHandler<GetUnreadCountQuery, int>
{
    private static readonly DistributedCacheEntryOptions Ttl =
        new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) };

    public async Task<int> Handle(GetUnreadCountQuery request, CancellationToken ct)
    {
        var key = $"notif:unread:{request.UserId}";
        var cached = await cache.GetStringAsync(key, ct);
        if (cached is not null && int.TryParse(cached, out var cachedCount))
            return cachedCount;

        var count = await db.Notifications
            .CountAsync(n => n.UserId == request.UserId && !n.IsRead, ct);

        await cache.SetStringAsync(key, count.ToString(), Ttl, ct);
        return count;
    }
}
