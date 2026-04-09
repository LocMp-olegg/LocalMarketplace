namespace LocMp.Catalog.Application.DTOs;

public sealed record ProductDto(
    Guid Id,
    Guid SellerId,
    Guid CategoryId,
    string Name,
    string? Description,
    decimal Price,
    string Unit,
    int StockQuantity,
    double? Latitude,
    double? Longitude,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    string? MainPhotoUrl,
    IReadOnlyList<ProductPhotoDto> Photos,
    IReadOnlyList<string> Tags
);
