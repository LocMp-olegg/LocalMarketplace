namespace LocMp.Contracts.Catalog;

public sealed record ProductRestockedEvent(
    Guid ProductId,
    Guid SellerId,
    string ProductName,
    Guid ShopId,
    int NewStock,
    DateTimeOffset OccurredAt) : IIntegrationEvent;
