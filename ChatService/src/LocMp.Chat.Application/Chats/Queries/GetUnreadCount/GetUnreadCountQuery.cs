using MediatR;

namespace LocMp.Chat.Application.Chats.Queries.GetUnreadCount;

public sealed record GetUnreadCountQuery(Guid UserId, bool IsAdmin = false) : IRequest<int>;