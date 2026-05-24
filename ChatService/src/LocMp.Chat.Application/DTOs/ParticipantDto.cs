using LocMp.Chat.Domain.Enums;

namespace LocMp.Chat.Application.DTOs;

public sealed record ParticipantDto(
    Guid UserId,
    ParticipantRole Role,
    DateTimeOffset JoinedAt,
    DateTimeOffset? LastReadAt
);