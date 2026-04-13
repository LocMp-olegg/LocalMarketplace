using MediatR;

namespace LocMp.Order.Application.Orders.Commands.Cart.ClearCart;

public sealed record ClearCartCommand(Guid UserId) : IRequest;
