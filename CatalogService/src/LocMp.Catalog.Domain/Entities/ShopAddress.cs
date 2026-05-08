namespace LocMp.Catalog.Domain.Entities;

public sealed class ShopAddress
{
    public string City { get; set; } = null!;
    public string Street { get; set; } = null!;
    public string HouseNumber { get; set; } = null!;
    public string? Apartment { get; set; }
    public string? Entrance { get; set; }
    public string? Floor { get; set; }
}
