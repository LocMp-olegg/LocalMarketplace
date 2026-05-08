namespace LocMp.Catalog.Api.Requests.Shops;

public sealed record ShopAddressRequest(
    string City,
    string Street,
    string HouseNumber,
    string? Apartment,
    string? Entrance,
    string? Floor
);
