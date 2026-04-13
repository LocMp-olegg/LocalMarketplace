using LocMp.Order.Application.DTOs;
using MediatR;

namespace LocMp.Order.Application.Orders.Commands.Cart.UpdateCartItemQuantity;

public sealed record UpdateCartItemQuantityCommand(
    Guid UserId,
    Guid CartItemId,
    int Quantity) : IRequest<CartDto>;
