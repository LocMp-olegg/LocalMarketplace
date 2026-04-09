namespace LocMp.Catalog.Application.DTOs;

public sealed record SellerDto(
    Guid Id,
    string DisplayName,
    string? AvatarUrl,
    decimal AverageRating,
    int ReviewCount,
    double? Latitude,
    double? Longitude,
    DateTimeOffset LastSyncedAt
);
