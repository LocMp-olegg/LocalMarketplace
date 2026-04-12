using LocMp.Order.Application.DTOs;
using MediatR;

namespace LocMp.Order.Application.Orders.Commands.Cart.AddToCart;

public sealed record AddToCartCommand(
    Guid UserId,
    Guid ProductId,
    int Quantity) : IRequest<CartDto>;
