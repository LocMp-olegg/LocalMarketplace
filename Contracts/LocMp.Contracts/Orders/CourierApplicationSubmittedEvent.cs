namespace LocMp.Contracts.Orders;

public sealed record CourierApplicationSubmittedEvent(
    Guid ApplicationId,
    Guid OrderId,
    Guid CourierId,
    string CourierName,
    DateTimeOffset OccurredAt) : IIntegrationEvent;