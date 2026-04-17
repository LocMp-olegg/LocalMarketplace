namespace LocMp.Contracts.Catalog;

public sealed record StockReleasedEvent(
    Guid ProductId,
    Guid SellerId,
    Guid? OrderId,
    int ReleasedQuantity,
    int NewQuantity,
    DateTimeOffset OccurredAt) : IIntegrationEvent;