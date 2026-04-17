namespace LocMp.Contracts.Catalog;

public sealed record ProductCreatedEvent(
    Guid ProductId,
    Guid SellerId,
    string ProductName,
    Guid? ShopId,
    string? ShopName,
    DateTimeOffset OccurredAt) : IIntegrationEvent;
