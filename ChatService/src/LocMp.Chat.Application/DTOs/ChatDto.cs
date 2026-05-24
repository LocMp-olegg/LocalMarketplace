using LocMp.Chat.Domain.Enums;

namespace LocMp.Chat.Application.DTOs;

public sealed record ChatDto(
    Guid Id,
    ChatType Type,
    ChatStatus Status,
    Guid? ReferenceId,
    DateTimeOffset? LastMessageAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ClosedAt,
    IReadOnlyList<ParticipantDto> Participants,
    string? InitiatorName,
    string? TargetName
);