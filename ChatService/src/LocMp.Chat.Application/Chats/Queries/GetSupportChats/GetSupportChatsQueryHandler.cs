using LocMp.BuildingBlocks.Application.Common;
using LocMp.Chat.Application.DTOs;
using LocMp.Chat.Application.Enums;
using LocMp.Chat.Application.Mapping;
using LocMp.Chat.Domain.Enums;
using LocMp.Chat.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Chat.Application.Chats.Queries.GetSupportChats;

public sealed class GetSupportChatsQueryHandler(ChatDbContext db)
    : IRequestHandler<GetSupportChatsQuery, PagedResult<ChatSummaryDto>>
{
    public async Task<PagedResult<ChatSummaryDto>> Handle(GetSupportChatsQuery request, CancellationToken ct)
    {
        var query = db.Chats
            .Include(c => c.Participants)
            .Include(c => c.Messages.Where(m => !m.IsDeleted))
            .Where(c => c.Type == ChatType.Support);

        if (request.Status.HasValue)
            query = query.Where(c => c.Status == request.Status.Value);

        if (request.InitiatorUserId.HasValue)
            query = query.Where(c => c.Participants
                .Any(p => p.Role == ParticipantRole.Initiator &&
                          p.UserId == request.InitiatorUserId.Value));

        var chats = await query.ToListAsync(ct);

        if (request.HasUnread.HasValue)
        {
            chats = chats
                .Where(c =>
                {
                    var adminParticipant = c.Participants
                        .FirstOrDefault(p => p.UserId == request.AdminId);
                    bool hasUnread;
                    if (adminParticipant is null)
                    {
                        hasUnread = c.Messages.Any(m =>
                            !m.IsDeleted && !m.IsRead && m.SenderId != request.AdminId);
                    }
                    else
                    {
                        hasUnread = c.Messages.Any(m =>
                            !m.IsDeleted &&
                            m.SenderId != adminParticipant.UserId &&
                            (adminParticipant.LastReadAt is null ||
                             m.SentAt > adminParticipant.LastReadAt));
                    }

                    return hasUnread == request.HasUnread.Value;
                })
                .ToList();
        }

        var total = chats.Count;

        chats = request.SortBy switch
        {
            SupportChatSortBy.OldestFirst => chats.OrderBy(c => c.CreatedAt).ToList(),
            SupportChatSortBy.HasUnread => chats
                .OrderByDescending(c =>
                {
                    var admin = c.Participants.FirstOrDefault(p => p.UserId == request.AdminId);
                    return admin is null
                        ? c.Messages.Count(m => !m.IsDeleted && !m.IsRead && m.SenderId != request.AdminId)
                        : c.Messages.Count(m =>
                            !m.IsDeleted &&
                            m.SenderId != admin.UserId &&
                            (admin.LastReadAt is null || m.SentAt > admin.LastReadAt));
                })
                .ThenByDescending(c => c.LastMessageAt ?? c.CreatedAt)
                .ToList(),
            _ => chats.OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt).ToList()
        };

        var items = chats
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => ChatMapper.ToSummaryDto(c, request.AdminId))
            .ToList();

        return PagedResult<ChatSummaryDto>.Create(items, total, request.Page, request.PageSize);
    }
}