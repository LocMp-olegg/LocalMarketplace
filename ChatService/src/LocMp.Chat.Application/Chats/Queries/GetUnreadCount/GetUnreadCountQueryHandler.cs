using LocMp.Chat.Domain.Enums;
using LocMp.Chat.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Chat.Application.Chats.Queries.GetUnreadCount;

public sealed class GetUnreadCountQueryHandler(ChatDbContext db)
    : IRequestHandler<GetUnreadCountQuery, int>
{
    public async Task<int> Handle(GetUnreadCountQuery request, CancellationToken ct)
    {
        var participants = await db.ChatParticipants
            .Where(p => p.UserId == request.UserId)
            .Select(p => new { p.ChatId, p.LastReadAt })
            .ToListAsync(ct);

        var joinedChatIds = participants.Select(p => p.ChatId).ToList();

        var unreadMessages = joinedChatIds.Count > 0
            ? await db.Messages
                .Where(m => joinedChatIds.Contains(m.ChatId) &&
                            m.SenderId != request.UserId &&
                            !m.IsRead &&
                            !m.IsDeleted)
                .Select(m => new { m.ChatId, m.SentAt })
                .ToListAsync(ct)
            : [];

        var total = 0;
        foreach (var participant in participants)
        {
            total += unreadMessages.Count(m =>
                m.ChatId == participant.ChatId &&
                (participant.LastReadAt is null || m.SentAt > participant.LastReadAt));
        }

        if (request.IsAdmin)
        {
            total += await db.Messages
                .Where(m => !joinedChatIds.Contains(m.ChatId) &&
                            m.SenderId != request.UserId &&
                            !m.IsDeleted &&
                            !m.IsRead &&
                            db.Chats.Any(c => c.Id == m.ChatId && c.Type == ChatType.Support))
                .CountAsync(ct);
        }

        return total;
    }
}