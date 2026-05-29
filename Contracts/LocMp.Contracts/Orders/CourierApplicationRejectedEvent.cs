namespace LocMp.Contracts.Orders;

public sealed record CourierApplicationRejectedEvent(
    Guid ApplicationId,
    Guid OrderId,
    Guid CourierId,
    DateTimeOffset OccurredAt) : IIntegrationEvent;