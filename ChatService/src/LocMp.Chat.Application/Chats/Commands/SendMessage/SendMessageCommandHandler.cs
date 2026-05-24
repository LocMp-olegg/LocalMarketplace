using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Chat.Application.Constants;
using LocMp.Chat.Application.DTOs;
using LocMp.Chat.Application.Interfaces;
using LocMp.Chat.Infrastructure.Services;
using LocMp.Chat.Application.Mapping;
using LocMp.Chat.Domain.Entities;
using LocMp.Chat.Domain.Enums;
using LocMp.Chat.Infrastructure.Persistence;
using LocMp.Contracts.Chat;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Chat.Application.Chats.Commands.SendMessage;

public sealed class SendMessageCommandHandler(
    ChatDbContext db,
    IChatEncryptionService encryption,
    IChatNotifier notifier,
    IStorageService storage,
    IEventBus eventBus)
    : IRequestHandler<SendMessageCommand, MessageDto>
{
    public async Task<MessageDto> Handle(SendMessageCommand request, CancellationToken ct)
    {
        var chat = await db.Chats
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == request.ChatId, ct)
            ?? throw new NotFoundException("Chat not found.");

        if (chat.Status == ChatStatus.Closed)
            throw new ForbiddenException("Cannot send messages to a closed chat.");

        var isParticipant = chat.Participants.Any(p => p.UserId == request.SenderId);

        if (!isParticipant && !request.IsAdmin)
            throw new ForbiddenException("You are not a participant of this chat.");

        if (chat.Type == ChatType.Support && request.IsAdmin && !isParticipant)
        {
            db.ChatParticipants.Add(new ChatParticipant(Guid.NewGuid())
            {
                ChatId = chat.Id,
                UserId = request.SenderId,
                Role = ParticipantRole.Admin,
                JoinedAt = DateTimeOffset.UtcNow
            });
        }

        var encryptedBody = encryption.Encrypt(request.Body ?? string.Empty, chat.EncryptionKey);

        var messageId = Guid.NewGuid();
        var message = new Message(messageId)
        {
            ChatId = chat.Id,
            SenderId = request.SenderId,
            SenderName = request.SenderName,
            Type = request.MessageType,
            EncryptedBody = encryptedBody,
            SentAt = DateTimeOffset.UtcNow
        };

        if (request.Attachments?.Count > 0)
            await ProcessAttachmentsAsync(message, chat.Id, request, ct);

        chat.LastMessageAt = message.SentAt;
        db.Messages.Add(message);
        await db.SaveChangesAsync(ct);

        var dto = ChatMapper.ToDto(message, encryption, chat.EncryptionKey, storage);

        var recipientIds = chat.Participants
            .Where(p => p.UserId != request.SenderId)
            .Select(p => p.UserId)
            .ToArray();

        await notifier.NotifyMessageSentAsync(chat.Id, dto, recipientIds, chat.Type == ChatType.Support, ct);

        await eventBus.PublishAsync(new ChatMessageSentEvent(
            chat.Id,
            message.Id,
            request.SenderId,
            request.SenderName,
            recipientIds,
            chat.Type.ToString(),
            chat.TargetName,
            message.SentAt), ct);

        return dto;
    }

    private async Task ProcessAttachmentsAsync(
        Message message, Guid chatId, SendMessageCommand request, CancellationToken ct)
    {
        foreach (var file in request.Attachments!)
        {
            var mediaType = AttachmentConstraints.AllowedImageMimeTypes.Contains(file.ContentType)
                ? MediaType.Image
                : MediaType.Video;

            var ext = Path.GetExtension(file.FileName) is { Length: > 0 } e ? e : string.Empty;
            var storageKey = $"chat-attachments/{chatId}/{message.Id}/{Guid.NewGuid()}{ext}";

            await using var stream = file.OpenReadStream();
            await storage.UploadAsync(stream, storageKey, file.ContentType, ct);

            message.Attachments.Add(new MessageAttachment(Guid.NewGuid())
            {
                MessageId = message.Id,
                FileName = Path.GetFileName(file.FileName),
                MimeType = file.ContentType,
                MediaType = mediaType,
                FileSize = file.Length,
                StorageKey = storageKey,
                UploadedAt = DateTimeOffset.UtcNow
            });
        }
    }
}
