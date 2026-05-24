using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Chat.Application.DTOs;
using LocMp.Chat.Application.Interfaces;
using LocMp.Chat.Infrastructure.Services;
using LocMp.Chat.Application.Mapping;
using LocMp.Chat.Domain.Entities;
using LocMp.Chat.Domain.Enums;
using LocMp.Chat.Infrastructure.Persistence;
using ChatEntity = LocMp.Chat.Domain.Entities.Chat;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Chat.Application.Chats.Commands.CreateChat;

public sealed class CreateChatCommandHandler(
    ChatDbContext db,
    IChatEncryptionService encryption,
    IChatNotifier notifier,
    IStorageService storage)
    : IRequestHandler<CreateChatCommand, ChatDto>
{
    public async Task<ChatDto> Handle(CreateChatCommand request, CancellationToken ct)
    {
        var existing = await FindExistingAsync(request, ct);
        if (existing is not null)
            return ChatMapper.ToChatDto(existing);

        var encryptionKey = encryption.GenerateChatKey();
        var now = DateTimeOffset.UtcNow;

        var chatId = Guid.NewGuid();
        var chat = new ChatEntity(chatId)
        {
            Type = request.Type,
            Status = ChatStatus.Active,
            ReferenceId = request.ReferenceId,
            EncryptionKey = encryptionKey,
            InitiatorName = string.IsNullOrWhiteSpace(request.InitiatorName) ? null : request.InitiatorName,
            TargetName = string.IsNullOrWhiteSpace(request.TargetUserName) ? null : request.TargetUserName,
            CreatedAt = now
        };

        chat.Participants.Add(new ChatParticipant(Guid.NewGuid())
        {
            ChatId = chatId,
            UserId = request.InitiatorId,
            Role = ParticipantRole.Initiator,
            JoinedAt = now
        });

        if (request.TargetUserId.HasValue)
        {
            chat.Participants.Add(new ChatParticipant(Guid.NewGuid())
            {
                ChatId = chatId,
                UserId = request.TargetUserId.Value,
                Role = ParticipantRole.Responder,
                JoinedAt = now
            });
        }

        if (!string.IsNullOrWhiteSpace(request.InitialMessage))
        {
            chat.Messages.Add(new Message(Guid.NewGuid())
            {
                ChatId = chatId,
                SenderId = request.InitiatorId,
                SenderName = request.InitiatorName,
                Type = MessageType.User,
                EncryptedBody = encryption.Encrypt(request.InitialMessage, encryptionKey),
                SentAt = now
            });
            chat.LastMessageAt = now;
        }

        db.Chats.Add(chat);
        await db.SaveChangesAsync(ct);

        if (chat.Messages.Count > 0)
        {
            var message = chat.Messages.First();
            var dto = ChatMapper.ToDto(message, encryption, chat.EncryptionKey, storage);
            var recipientIds = chat.Participants
                .Where(p => p.UserId != request.InitiatorId)
                .Select(p => p.UserId)
                .ToArray();
            await notifier.NotifyMessageSentAsync(
                chat.Id, dto, recipientIds, chat.Type == ChatType.Support, ct);
        }

        return ChatMapper.ToChatDto(chat);
    }

    private async Task<ChatEntity?> FindExistingAsync(CreateChatCommand request, CancellationToken ct)
    {
        return request.Type switch
        {
            ChatType.Order when request.ReferenceId.HasValue =>
                await db.Chats
                    .Include(c => c.Participants)
                    .FirstOrDefaultAsync(c =>
                        c.Type == ChatType.Order && c.ReferenceId == request.ReferenceId, ct),

            ChatType.Shop when request.ReferenceId.HasValue =>
                await db.Chats
                    .Include(c => c.Participants)
                    .Where(c => c.Type == ChatType.Shop && c.ReferenceId == request.ReferenceId)
                    .FirstOrDefaultAsync(c =>
                        c.Participants.Any(p => p.UserId == request.InitiatorId), ct),

            ChatType.Direct when request.TargetUserId.HasValue =>
                await db.Chats
                    .Include(c => c.Participants)
                    .Where(c => c.Type == ChatType.Direct)
                    .FirstOrDefaultAsync(c =>
                        c.Participants.Any(p => p.UserId == request.InitiatorId) &&
                        c.Participants.Any(p => p.UserId == request.TargetUserId.Value), ct),

            _ => null
        };
    }
}