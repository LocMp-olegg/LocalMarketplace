using LocMp.Catalog.Domain.Enums;

namespace LocMp.Catalog.Application.DTOs;

public sealed record ShopDto(
    Guid Id,
    Guid SellerId,
    string BusinessName,
    string PhoneNumber,
    string Email,
    string? Description,
    string? Inn,
    BusinessType BusinessType,
    string? WorkingHours,
    int? ServiceRadiusMeters,
    double? Latitude,
    double? Longitude,
    string? AvatarUrl,
    bool IsVerified,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
