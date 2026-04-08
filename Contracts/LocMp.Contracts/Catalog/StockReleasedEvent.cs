namespace LocMp.Contracts.Catalog;

public sealed record StockReleasedEvent(
    Guid ProductId,
    Guid OrderId,
    int ReleasedQuantity,
    DateTimeOffset OccurredAt) : IIntegrationEvent;