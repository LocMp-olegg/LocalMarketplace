using LocMp.Chat.Domain.Enums;

namespace LocMp.Chat.Application.DTOs;

public sealed record AttachmentDto(
    Guid Id,
    string FileName,
    string MimeType,
    MediaType MediaType,
    long FileSize,
    string Url
);