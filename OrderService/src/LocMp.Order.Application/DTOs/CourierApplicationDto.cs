using LocMp.Order.Domain.Enums;

namespace LocMp.Order.Application.DTOs;

public sealed record CourierApplicationDto(
    Guid Id,
    Guid OrderId,
    Guid CourierId,
    string CourierName,
    string CourierPhone,
    double? DistanceToShopMeters,
    CourierApplicationStatus Status,
    DateTimeOffset AppliedAt,
    DateTimeOffset? UpdatedAt);