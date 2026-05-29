namespace LocMp.Contracts.Orders;

public sealed record CourierApplicationApprovedEvent(
    Guid ApplicationId,
    Guid OrderId,
    Guid CourierId,
    Guid SellerId,
    DateTimeOffset OccurredAt) : IIntegrationEvent;