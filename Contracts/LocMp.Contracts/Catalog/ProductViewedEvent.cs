namespace LocMp.Contracts.Catalog;

public sealed record ProductViewedEvent(
    Guid ProductId,
    Guid SellerId,
    Guid? ViewerId,
    DateTimeOffset OccurredAt) : IIntegrationEvent;