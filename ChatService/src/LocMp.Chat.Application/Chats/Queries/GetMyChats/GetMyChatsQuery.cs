using LocMp.BuildingBlocks.Application.Common;
using LocMp.Chat.Application.DTOs;
using LocMp.Chat.Domain.Enums;
using MediatR;

namespace LocMp.Chat.Application.Chats.Queries.GetMyChats;

public sealed record GetMyChatsQuery(
    Guid UserId,
    ChatType? Type,
    ChatStatus? Status,
    int Page,
    int PageSize
) : IRequest<PagedResult<ChatSummaryDto>>;