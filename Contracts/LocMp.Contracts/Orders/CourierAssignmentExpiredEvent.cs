namespace LocMp.Contracts.Orders;

public sealed record CourierAssignmentExpiredEvent(
    Guid OrderId,
    Guid CourierId,
    Guid SellerId,
    DateTimeOffset OccurredAt) : IIntegrationEvent;