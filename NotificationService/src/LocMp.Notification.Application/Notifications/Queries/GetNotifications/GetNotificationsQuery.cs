using LocMp.BuildingBlocks.Application.Common;
using LocMp.Notification.Application.DTOs;
using MediatR;

namespace LocMp.Notification.Application.Notifications.Queries.GetNotifications;

public sealed record GetNotificationsQuery(
    Guid UserId,
    bool? OnlyUnread,
    int Page,
    int PageSize) : IRequest<PagedResult<NotificationDto>>;
