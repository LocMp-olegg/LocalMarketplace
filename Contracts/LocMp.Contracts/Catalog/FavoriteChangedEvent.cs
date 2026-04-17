namespace LocMp.Contracts.Catalog;

public sealed record FavoriteChangedEvent(
    Guid ProductId,
    Guid SellerId,
    Guid UserId,
    bool IsAdded,
    DateTimeOffset OccurredAt) : IIntegrationEvent;
