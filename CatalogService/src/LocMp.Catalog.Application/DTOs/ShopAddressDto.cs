namespace LocMp.Catalog.Application.DTOs;

public sealed record ShopAddressDto
{
    public string City { get; init; } = null!;
    public string Street { get; init; } = null!;
    public string HouseNumber { get; init; } = null!;
    public string? Apartment { get; init; }
    public string? Entrance { get; init; }
    public string? Floor { get; init; }
}
