using AutoMapper;
using AutoMapper.QueryableExtensions;
using LocMp.BuildingBlocks.Application.Common;
using LocMp.Notification.Application.DTOs;
using LocMp.Notification.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Notification.Application.Notifications.Queries.GetNotifications;

public sealed class GetNotificationsQueryHandler(NotificationDbContext db, IMapper mapper)
    : IRequestHandler<GetNotificationsQuery, PagedResult<NotificationDto>>
{
    public async Task<PagedResult<NotificationDto>> Handle(GetNotificationsQuery request, CancellationToken ct)
    {
        var query = db.Notifications
            .Where(n => n.UserId == request.UserId);

        if (request.OnlyUnread == true)
            query = query.Where(n => !n.IsRead);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<NotificationDto>(mapper.ConfigurationProvider)
            .ToListAsync(ct);

        return PagedResult<NotificationDto>.Create(items, total, request.Page, request.PageSize);
    }
}
