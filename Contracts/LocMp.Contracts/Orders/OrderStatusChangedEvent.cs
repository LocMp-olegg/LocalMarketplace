namespace LocMp.Contracts.Orders;

public sealed record OrderStatusChangedEvent(
    Guid OrderId,
    Guid BuyerId,
    Guid SellerId,
    string FromStatus,
    string ToStatus,
    DateTimeOffset OccurredAt) : IIntegrationEvent;