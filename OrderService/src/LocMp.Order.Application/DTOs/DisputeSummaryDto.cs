using LocMp.Order.Domain.Enums;

namespace LocMp.Order.Application.DTOs;

public sealed record DisputeSummaryDto(
    Guid Id,
    Guid OrderId,
    Guid InitiatorId,
    string Reason,
    DisputeStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ResolvedAt);
