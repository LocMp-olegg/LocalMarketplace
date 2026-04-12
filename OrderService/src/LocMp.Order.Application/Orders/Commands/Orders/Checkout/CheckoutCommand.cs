using LocMp.Order.Application.DTOs;
using LocMp.Order.Domain.Enums;
using MediatR;

namespace LocMp.Order.Application.Orders.Commands.Orders.Checkout;

public sealed record CheckoutCommand(
    Guid UserId,
    DeliveryType DeliveryType,
    string? BuyerComment,
    DeliveryAddressData? DeliveryAddress) : IRequest<OrderDto>;

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
