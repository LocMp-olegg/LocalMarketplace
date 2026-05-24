using LocMp.BuildingBlocks.Application.Common;
using LocMp.Chat.Application.DTOs;
using MediatR;

namespace LocMp.Chat.Application.Chats.Queries.GetChatMessages;

public sealed record GetChatMessagesQuery(
    Guid ChatId,
    Guid UserId,
    bool IsAdmin,
    int Page,
    int PageSize
) : IRequest<PagedResult<MessageDto>>;