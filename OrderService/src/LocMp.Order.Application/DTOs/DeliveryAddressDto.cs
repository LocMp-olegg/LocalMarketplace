namespace LocMp.Order.Application.DTOs;

public sealed record DeliveryAddressDto(
    string City,
    string Street,
    string HouseNumber,
    string? Apartment,
    string? Entrance,
    int? Floor,
    double? Latitude,
    double? Longitude,
    string RecipientName,
    string RecipientPhone);
