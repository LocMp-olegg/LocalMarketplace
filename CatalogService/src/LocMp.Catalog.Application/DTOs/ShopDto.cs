using LocMp.Catalog.Domain.Enums;

namespace LocMp.Catalog.Application.DTOs;

public sealed record ShopDto
{
    public Guid Id { get; init; }
    public Guid SellerId { get; init; }
    public string BusinessName { get; init; } = null!;
    public string PhoneNumber { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string? Description { get; init; }
    public string? Inn { get; init; }
    public BusinessType BusinessType { get; init; }
    public string? WorkingHours { get; init; }
    public int? ServiceRadiusMeters { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public string? AvatarUrl { get; init; }
    public bool IsVerified { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}