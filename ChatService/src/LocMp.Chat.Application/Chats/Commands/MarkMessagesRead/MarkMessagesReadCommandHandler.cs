using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Chat.Application.Interfaces;
using LocMp.Chat.Domain.Entities;
using LocMp.Chat.Domain.Enums;
using LocMp.Chat.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Chat.Application.Chats.Commands.MarkMessagesRead;

public sealed class MarkMessagesReadCommandHandler(ChatDbContext db, IChatNotifier notifier)
    : IRequestHandler<MarkMessagesReadCommand>
{
    public async Task Handle(MarkMessagesReadCommand request, CancellationToken ct)
    {
        var participant = await db.ChatParticipants
            .FirstOrDefaultAsync(p => p.ChatId == request.ChatId && p.UserId == request.UserId, ct);

        if (participant is null && !request.IsAdmin)
            throw new ForbiddenException("You are not a participant of this chat.");

        var now = DateTimeOffset.UtcNow;
        if (participant is not null)
        {
            participant.LastReadAt = now;
        }
        else if (request.IsAdmin)
        {
            var adminParticipant = new ChatParticipant(Guid.NewGuid())
            {
                ChatId = request.ChatId,
                UserId = request.UserId,
                Role = ParticipantRole.Admin,
                JoinedAt = now,
                LastReadAt = now,
            };
            db.ChatParticipants.Add(adminParticipant);
        }

        var affected = await db.Messages
            .Where(m => m.ChatId == request.ChatId &&
                        m.SenderId != request.UserId &&
                        !m.IsRead &&
                        !m.IsDeleted)
            .ExecuteUpdateAsync(s => s
                .SetProperty(m => m.IsRead, true)
                .SetProperty(m => m.ReadAt, now), ct);

        await db.SaveChangesAsync(ct);

        if (affected > 0)
            await notifier.NotifyMessagesReadAsync(request.ChatId, request.UserId, ct);
    }
}