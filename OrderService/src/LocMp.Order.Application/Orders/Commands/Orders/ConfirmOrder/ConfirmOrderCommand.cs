using MediatR;

namespace LocMp.Order.Application.Orders.Commands.Orders.ConfirmOrder;

public sealed record ConfirmOrderCommand(Guid OrderId, Guid SellerId) : IRequest;
