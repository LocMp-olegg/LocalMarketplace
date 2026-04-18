namespace LocMp.Contracts.Catalog;

public sealed record ProductViewedEvent(
    Guid ProductId,
    Guid SellerId,
    string ProductName,
    Guid? ViewerId,
    DateTimeOffset OccurredAt) : IIntegrationEvent;