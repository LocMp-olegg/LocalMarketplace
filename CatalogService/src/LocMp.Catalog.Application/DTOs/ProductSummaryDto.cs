namespace LocMp.Catalog.Application.DTOs;

public sealed record ProductSummaryDto(
    Guid Id,
    Guid SellerId,
    Guid? ShopId,
    Guid CategoryId,
    string Name,
    decimal Price,
    string Unit,
    int StockQuantity,
    bool IsActive,
    double? Latitude,
    double? Longitude,
    string? MainPhotoUrl,
    double? DistanceMeters,
    IReadOnlyList<string> Tags,
    bool IsMadeToOrder,
    int? LeadTimeDays
);
