using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Notification.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace LocMp.Notification.Application.Notifications.Commands.MarkNotificationRead;

public sealed class MarkNotificationReadCommandHandler(NotificationDbContext db, IDistributedCache cache)
    : IRequestHandler<MarkNotificationReadCommand>
{
    public async Task Handle(MarkNotificationReadCommand request, CancellationToken ct)
    {
        var notification = await db.Notifications
                               .FirstOrDefaultAsync(n => n.Id == request.NotificationId, ct)
                           ?? throw new NotFoundException($"Notification '{request.NotificationId}' not found.");

        if (notification.UserId != request.UserId)
            throw new ForbiddenException("You cannot access this notification.");

        if (notification.IsRead)
            return;

        notification.IsRead = true;
        notification.ReadAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync($"notif:unread:{request.UserId}", ct);
    }
}
