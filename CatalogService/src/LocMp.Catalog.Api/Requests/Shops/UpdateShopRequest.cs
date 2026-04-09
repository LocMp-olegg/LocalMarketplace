using LocMp.Catalog.Domain.Enums;

namespace LocMp.Catalog.Api.Requests.Shops;

public sealed record UpdateShopRequest(
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
    bool IsActive
);
