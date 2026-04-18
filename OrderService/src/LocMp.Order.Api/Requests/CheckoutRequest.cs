using LocMp.Order.Domain.Enums;

namespace LocMp.Order.Api.Requests;

public sealed record CheckoutRequest(
    string? BuyerComment,
    IReadOnlyList<GroupDeliveryRequest> Groups);

public sealed record GroupDeliveryRequest(
    Guid SellerId,
    Guid? ShopId,
    DeliveryType DeliveryType,
    DeliveryAddressRequest? DeliveryAddress,
    IReadOnlyList<Guid>? SelectedItemIds = null);

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
