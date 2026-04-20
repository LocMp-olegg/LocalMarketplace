namespace LocMp.Contracts.Orders;

public sealed record DisputeOpenedEvent(
    Guid DisputeId,
    Guid OrderId,
    Guid BuyerId,
    Guid SellerId,
    Guid InitiatorId,
    DisputeType DisputeType,
    DateTimeOffset OccurredAt) : IIntegrationEvent;