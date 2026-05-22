namespace LocMp.Catalog.Application.DTOs;

public sealed record ShopMapDto(
    Guid Id,
    string Name,
    double Latitude,
    double Longitude,
    string? AvatarUrl,
    decimal AverageRating,
    int ReviewCount,
    double? ServiceRadiusKm,
    bool IsActive
);