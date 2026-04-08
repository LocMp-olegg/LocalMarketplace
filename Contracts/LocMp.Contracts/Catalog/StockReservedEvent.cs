namespace LocMp.Contracts.Catalog;

public sealed record StockReservedEvent(
    Guid ProductId,
    Guid OrderId,
    int ReservedQuantity,
    DateTimeOffset OccurredAt) : IIntegrationEvent;