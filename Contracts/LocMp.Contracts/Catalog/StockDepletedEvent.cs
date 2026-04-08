namespace LocMp.Contracts.Catalog;

public sealed record StockDepletedEvent(
    Guid ProductId,
    Guid SellerId,
    string ProductName,
    DateTimeOffset OccurredAt) : IIntegrationEvent;