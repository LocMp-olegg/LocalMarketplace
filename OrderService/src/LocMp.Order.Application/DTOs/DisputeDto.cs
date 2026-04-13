using LocMp.Order.Domain.Enums;

namespace LocMp.Order.Application.DTOs;

public sealed record DisputeDto(
    Guid Id,
    Guid InitiatorId,
    string Reason,
    DisputeStatus Status,
    string? Resolution,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ResolvedAt,
    IReadOnlyList<DisputePhotoDto> Photos);
