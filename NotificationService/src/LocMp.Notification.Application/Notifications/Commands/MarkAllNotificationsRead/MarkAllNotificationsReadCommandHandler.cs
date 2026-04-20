using LocMp.Notification.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace LocMp.Notification.Application.Notifications.Commands.MarkAllNotificationsRead;

public sealed class MarkAllNotificationsReadCommandHandler(NotificationDbContext db, IDistributedCache cache)
    : IRequestHandler<MarkAllNotificationsReadCommand>
{
    public async Task Handle(MarkAllNotificationsReadCommand request, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        await db.Notifications
            .Where(n => n.UserId == request.UserId && !n.IsRead)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.ReadAt, now), ct);

        await cache.RemoveAsync($"notif:unread:{request.UserId}", ct);
    }
}
