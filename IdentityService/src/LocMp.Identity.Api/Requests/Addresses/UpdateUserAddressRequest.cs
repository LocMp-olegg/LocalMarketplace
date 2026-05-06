namespace LocMp.Identity.Api.Requests.Addresses;

public sealed record UpdateUserAddressRequest(
    string Title,
    string City,
    string Street,
    string HouseNumber,
    string? Apartment,
    string? Entrance,
    string? Floor,
    double? Latitude,
    double? Longitude
);
