using LocMp.BuildingBlocks.Application.Common;
using LocMp.Chat.Application.DTOs;
using LocMp.Chat.Application.Mapping;
using LocMp.Chat.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Chat.Application.Chats.Queries.GetMyChats;

public sealed class GetMyChatsQueryHandler(ChatDbContext db)
    : IRequestHandler<GetMyChatsQuery, PagedResult<ChatSummaryDto>>
{
    public async Task<PagedResult<ChatSummaryDto>> Handle(GetMyChatsQuery request, CancellationToken ct)
    {
        var query = db.Chats
            .Include(c => c.Participants)
            .Include(c => c.Messages.Where(m => !m.IsDeleted))
            .Where(c => c.Participants.Any(p => p.UserId == request.UserId));

        if (request.Type.HasValue)
            query = query.Where(c => c.Type == request.Type.Value);

        if (request.Status.HasValue)
            query = query.Where(c => c.Status == request.Status.Value);

        var total = await query.CountAsync(ct);

        var chats = await query
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var items = chats
            .Select(c => ChatMapper.ToSummaryDto(c, request.UserId))
            .ToList();

        return PagedResult<ChatSummaryDto>.Create(items, total, request.Page, request.PageSize);
    }
}