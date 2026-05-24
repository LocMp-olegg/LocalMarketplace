using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Chat.Application.Interfaces;
using LocMp.Chat.Domain.Enums;
using LocMp.Chat.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Chat.Application.Chats.Commands.CloseChat;

public sealed class CloseChatCommandHandler(ChatDbContext db, IChatNotifier notifier)
    : IRequestHandler<CloseChatCommand>
{
    public async Task Handle(CloseChatCommand request, CancellationToken ct)
    {
        var chat = await db.Chats
                       .Include(c => c.Participants)
                       .FirstOrDefaultAsync(c => c.Id == request.ChatId, ct)
                   ?? throw new NotFoundException("Chat not found.");

        if (chat.Status == ChatStatus.Closed)
            return;

        var isParticipant = chat.Participants.Any(p => p.UserId == request.UserId);
        if (!isParticipant && !request.IsAdmin)
            throw new ForbiddenException("You cannot close this chat.");

        chat.Status = ChatStatus.Closed;
        chat.ClosedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
        await notifier.NotifyChatClosedAsync(chat.Id, ct);
    }
}