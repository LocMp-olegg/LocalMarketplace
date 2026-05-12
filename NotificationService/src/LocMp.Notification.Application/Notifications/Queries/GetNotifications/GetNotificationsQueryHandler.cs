using LocMp.BuildingBlocks.Application.Common;
using LocMp.Notification.Application.DTOs;
using LocMp.Notification.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Notification.Application.Notifications.Queries.GetNotifications;

public sealed class GetNotificationsQueryHandler(NotificationDbContext db)
    : IRequestHandler<GetNotificationsQuery, PagedResult<NotificationDto>>
{
    public async Task<PagedResult<NotificationDto>> Handle(GetNotificationsQuery request, CancellationToken ct)
    {
        var query = db.Notifications
            .Where(n => n.UserId == request.UserId);

        if (request.OnlyUnread == true)
            query = query.Where(n => !n.IsRead);

        var total = await query.CountAsync(ct);

        var raw = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var items = raw.Select(n => new NotificationDto(
            n.Id, n.Type, n.Title, n.Body, n.IsRead, n.ReadAt, n.CreatedAt,
            n.Payload?.RootElement)).ToList();

        return PagedResult<NotificationDto>.Create(items, total, request.Page, request.PageSize);
    }
}