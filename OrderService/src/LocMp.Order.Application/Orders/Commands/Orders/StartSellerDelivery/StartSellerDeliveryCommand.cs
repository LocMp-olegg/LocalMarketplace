using MediatR;

namespace LocMp.Order.Application.Orders.Commands.Orders.StartSellerDelivery;

public sealed record StartSellerDeliveryCommand(
    Guid SellerId,
    Guid OrderId) : IRequest;