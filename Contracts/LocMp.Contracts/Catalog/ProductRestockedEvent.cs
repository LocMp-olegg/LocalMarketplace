namespace LocMp.Contracts.Catalog;

public sealed record ProductRestockedEvent(
    Guid ProductId,
    Guid SellerId,
    string ProductName,
    Guid ShopId,
    int NewStock,
    IReadOnlyList<Guid> FavoritedByUserIds,
    DateTimeOffset OccurredAt) : IIntegrationEvent;
