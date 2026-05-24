using LocMp.Chat.Domain.Enums;

namespace LocMp.Chat.Application.DTOs;

public sealed record ChatSummaryDto(
    Guid Id,
    ChatType Type,
    ChatStatus Status,
    Guid? ReferenceId,
    DateTimeOffset? LastMessageAt,
    int UnreadCount,
    DateTimeOffset CreatedAt,
    IReadOnlyList<ParticipantDto> Participants,
    string? InitiatorName,
    string? TargetName
);