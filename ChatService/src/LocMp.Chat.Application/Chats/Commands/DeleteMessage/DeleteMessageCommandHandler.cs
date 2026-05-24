using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Chat.Application.Interfaces;
using LocMp.Chat.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Chat.Application.Chats.Commands.DeleteMessage;

public sealed class DeleteMessageCommandHandler(
    ChatDbContext db,
    IChatNotifier notifier,
    IStorageService storage)
    : IRequestHandler<DeleteMessageCommand>
{
    public async Task Handle(DeleteMessageCommand request, CancellationToken ct)
    {
        var message = await db.Messages
                          .Include(m => m.Attachments)
                          .FirstOrDefaultAsync(m => m.Id == request.MessageId, ct)
                      ?? throw new NotFoundException("Message not found.");

        if (!request.IsAdmin && message.SenderId != request.UserId)
            throw new ForbiddenException("You can only delete your own messages.");

        foreach (var attachment in message.Attachments)
            await storage.DeleteAsync(attachment.StorageKey, ct);

        message.IsDeleted = true;
        message.DeletedAt = DateTimeOffset.UtcNow;
        message.EncryptedBody = string.Empty;

        await db.SaveChangesAsync(ct);
        await notifier.NotifyMessageDeletedAsync(message.ChatId, message.Id, ct);
    }
}