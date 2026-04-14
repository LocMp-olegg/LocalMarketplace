using LocMp.Order.Domain.Enums;

namespace LocMp.Order.Application.DTOs;

public sealed record DisputeSummaryDto(
    Guid Id,
    Guid OrderId,
    Guid InitiatorId,
    string Reason,
    DisputeStatus Status,
    DisputeOutcome? Outcome,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ResolvedAt);