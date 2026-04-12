namespace LocMp.Order.Application.DTOs;

public sealed record CourierAssignmentDto(
    Guid Id,
    Guid CourierId,
    string CourierName,
    string CourierPhone,
    DateTimeOffset AssignedAt,
    DateTimeOffset? PickedUpAt,
    DateTimeOffset? DeliveredAt);
