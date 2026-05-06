namespace LocMp.Identity.Application.DTOs.UserAddress;

public sealed record UserAddressDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Title { get; init; } = null!;
    public string City { get; init; } = null!;
    public string Street { get; init; } = null!;
    public string HouseNumber { get; init; } = null!;
    public string? Apartment { get; init; }
    public string? Entrance { get; init; }
    public string? Floor { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public bool IsDefault { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
