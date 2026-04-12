namespace LocMp.Order.Infrastructure.Clients;

public sealed record ProductSnapshotDto(
    Guid Id,
    Guid SellerId,
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    string? MainPhotoUrl,
    bool IsActive);
