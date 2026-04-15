using LocMp.Contracts.Orders;
using LocMp.Order.Domain.Enums;

namespace LocMp.Order.Application.DTOs;

public sealed record DisputeDto(
    Guid Id,
    Guid InitiatorId,
    string Reason,
    DisputeStatus Status,
    DisputeOutcome? Outcome,
    string? Resolution,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ResolvedAt,
    IReadOnlyList<DisputePhotoDto> Photos);