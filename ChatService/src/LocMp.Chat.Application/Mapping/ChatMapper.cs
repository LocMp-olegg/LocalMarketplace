using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Chat.Application.DTOs;
using LocMp.Chat.Infrastructure.Services;
using LocMp.Chat.Domain.Entities;
using ChatEntity = LocMp.Chat.Domain.Entities.Chat;

namespace LocMp.Chat.Application.Mapping;

internal static class ChatMapper
{
    public static MessageDto ToDto(
        Message message,
        IChatEncryptionService encryption,
        string chatEncryptionKey,
        IStorageService storage)
    {
        string body;
        if (message.IsDeleted)
        {
            body = string.Empty;
        }
        else
        {
            body = string.IsNullOrWhiteSpace(message.EncryptedBody)
                ? string.Empty
                : encryption.Decrypt(message.EncryptedBody, chatEncryptionKey);
        }

        var attachments = message.Attachments
            .Select(a => ToDto(a, storage))
            .ToList();

        return new MessageDto(
            message.Id,
            message.ChatId,
            message.SenderId,
            message.SenderName,
            message.Type,
            body,
            message.SentAt,
            message.IsRead,
            message.IsDeleted,
            attachments);
    }

    public static AttachmentDto ToDto(MessageAttachment attachment, IStorageService storage)
        => new(
            attachment.Id,
            attachment.FileName,
            attachment.MimeType,
            attachment.MediaType,
            attachment.FileSize,
            storage.GetUrl(attachment.StorageKey));

    public static ChatDto ToChatDto(ChatEntity chat)
        => new(
            chat.Id,
            chat.Type,
            chat.Status,
            chat.ReferenceId,
            chat.LastMessageAt,
            chat.CreatedAt,
            chat.ClosedAt,
            chat.Participants.Select(ToParticipantDto).ToList(),
            chat.InitiatorName,
            chat.TargetName);

    public static ChatSummaryDto ToSummaryDto(ChatEntity chat, Guid currentUserId)
    {
        var participant = chat.Participants.FirstOrDefault(p => p.UserId == currentUserId);
        var unreadCount = participant is not null
            ? chat.Messages.Count(m =>
                !m.IsDeleted &&
                m.SenderId != currentUserId &&
                (participant.LastReadAt is null || m.SentAt > participant.LastReadAt))
            : currentUserId != Guid.Empty
                ? chat.Messages.Count(m => !m.IsDeleted && !m.IsRead && m.SenderId != currentUserId)
                : 0;

        return new ChatSummaryDto(
            chat.Id,
            chat.Type,
            chat.Status,
            chat.ReferenceId,
            chat.LastMessageAt,
            unreadCount,
            chat.CreatedAt,
            chat.Participants.Select(ToParticipantDto).ToList(),
            chat.InitiatorName,
            chat.TargetName);
    }

    public static ParticipantDto ToParticipantDto(ChatParticipant p)
        => new(p.UserId, p.Role, p.JoinedAt, p.LastReadAt);
}