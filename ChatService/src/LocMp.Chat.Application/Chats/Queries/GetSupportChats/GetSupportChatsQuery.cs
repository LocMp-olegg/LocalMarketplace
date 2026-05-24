using LocMp.BuildingBlocks.Application.Common;
using LocMp.Chat.Application.DTOs;
using LocMp.Chat.Application.Enums;
using LocMp.Chat.Domain.Enums;
using MediatR;

namespace LocMp.Chat.Application.Chats.Queries.GetSupportChats;

public sealed record GetSupportChatsQuery(
    Guid AdminId,
    Guid? InitiatorUserId,
    ChatStatus? Status,
    SupportChatSortBy SortBy,
    bool? HasUnread,
    int Page,
    int PageSize
) : IRequest<PagedResult<ChatSummaryDto>>;