using LocMp.Order.Application.DTOs;
using LocMp.Order.Domain.Enums;
using MediatR;

namespace LocMp.Order.Application.Orders.Commands.Orders.Checkout;

public sealed record CheckoutCommand(
    Guid UserId,
    string? BuyerComment,
    IReadOnlyList<GroupDeliverySettings> Groups) : IRequest<IReadOnlyList<OrderDto>>;

public sealed record GroupDeliverySettings(
    Guid SellerId,
    Guid? ShopId,
    DeliveryType DeliveryType,
    DeliveryAddressData? DeliveryAddress,
    IReadOnlyList<Guid>? SelectedItemIds = null);

public sealed record DeliveryAddressData(
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
