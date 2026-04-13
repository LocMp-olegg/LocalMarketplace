using LocMp.Order.Application.DTOs;
using MediatR;

namespace LocMp.Order.Application.Orders.Queries.Cart.GetCartByUser;

public sealed record GetCartByUserQuery(Guid UserId) : IRequest<CartDto?>;
