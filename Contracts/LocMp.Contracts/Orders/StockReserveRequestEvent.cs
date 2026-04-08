namespace LocMp.Contracts.Orders;

public sealed record StockReserveRequestEvent(
    Guid ProductId,
    Guid OrderId,
    int Quantity,
    DateTimeOffset OccurredAt) : IIntegrationEvent;