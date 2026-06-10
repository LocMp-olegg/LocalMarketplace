using MediatR;

namespace LocMp.Order.Application.Orders.Commands.Orders.MarkSellerPickedUp;

public sealed record MarkSellerPickedUpCommand(Guid OrderId, Guid SellerId) : IRequest;