using MediatR;

namespace LocMp.Order.Application.Orders.Commands.Orders.CompleteOrder;

public sealed record CompleteOrderCommand(Guid OrderId, Guid BuyerId) : IRequest;
