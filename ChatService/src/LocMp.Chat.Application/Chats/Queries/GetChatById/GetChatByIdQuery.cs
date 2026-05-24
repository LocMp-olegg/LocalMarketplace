using LocMp.Chat.Application.DTOs;
using MediatR;

namespace LocMp.Chat.Application.Chats.Queries.GetChatById;

public sealed record GetChatByIdQuery(Guid ChatId, Guid UserId, bool IsAdmin) : IRequest<ChatDto>;