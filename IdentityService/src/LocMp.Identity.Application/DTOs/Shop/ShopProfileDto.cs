using LocMp.Identity.Domain.Enums;

namespace LocMp.Identity.Application.DTOs.Shop;

public sealed record ShopProfileDto(
    Guid Id,
    Guid UserId,
    string BusinessName,
    string PhoneNumber,
    string Email,
    string? Description,
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