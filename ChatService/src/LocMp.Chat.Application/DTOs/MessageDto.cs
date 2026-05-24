using LocMp.Chat.Domain.Enums;

namespace LocMp.Chat.Application.DTOs;

public sealed record MessageDto(
    Guid Id,
    Guid ChatId,
    Guid SenderId,
    string SenderName,
    MessageType Type,
    string Body,
    DateTimeOffset SentAt,
    bool IsRead,
    bool IsDeleted,
    IReadOnlyList<AttachmentDto> Attachments
);