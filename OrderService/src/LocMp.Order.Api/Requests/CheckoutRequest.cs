using LocMp.Order.Domain.Enums;

namespace LocMp.Order.Api.Requests;

public sealed record CheckoutRequest(
    DeliveryType DeliveryType,
    string? BuyerComment,
    DeliveryAddressRequest? DeliveryAddress);

public sealed record DeliveryAddressRequest(
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
