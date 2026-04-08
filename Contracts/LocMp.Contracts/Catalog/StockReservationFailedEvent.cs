namespace LocMp.Contracts.Catalog;

public sealed record StockReservationFailedEvent(
    Guid ProductId,
    Guid OrderId,
    string Reason,
    DateTimeOffset OccurredAt) : IIntegrationEvent;
