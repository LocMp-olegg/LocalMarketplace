using LocMp.Identity.Domain.Enums;

namespace LocMp.Identity.Api.Requests.Shop;

public sealed class CreateShopRequest
{
    public string BusinessName { get; init; } = null!;
    public string PhoneNumber { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string? Description { get; init; }
    public string? Inn { get; init; }
    public BusinessType BusinessType { get; init; } = BusinessType.Individual;
    public string? WorkingHours { get; init; }
    public int? ServiceRadiusMeters { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
}
