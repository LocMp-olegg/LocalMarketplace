using MediatR;

namespace LocMp.Notification.Application.Notifications.Queries.GetUnreadCount;

public sealed record GetUnreadCountQuery(Guid UserId) : IRequest<int>;
