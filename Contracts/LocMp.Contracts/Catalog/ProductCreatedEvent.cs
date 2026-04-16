namespace LocMp.Contracts.Catalog;

public sealed record ProductCreatedEvent(
    Guid ProductId,
    Guid SellerId,
    string ProductName,
    DateTimeOffset OccurredAt) : IIntegrationEvent;
