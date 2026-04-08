namespace LocMp.Contracts.Orders;

public sealed record OrderPlacedEvent(
    Guid OrderId,
    Guid BuyerId,
    Guid SellerId,
    decimal TotalAmount,
    DateTimeOffset OccurredAt) : IIntegrationEvent;