using MediatR;

namespace LocMp.Order.Application.Orders.Commands.Cart.RemoveFromCart;

public sealed record RemoveFromCartCommand(Guid UserId, Guid CartItemId) : IRequest;
