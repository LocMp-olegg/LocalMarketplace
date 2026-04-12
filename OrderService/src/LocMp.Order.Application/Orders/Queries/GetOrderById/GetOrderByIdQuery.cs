using LocMp.Order.Application.DTOs;
using MediatR;

namespace LocMp.Order.Application.Orders.Queries.GetOrderById;

public sealed record GetOrderByIdQuery(Guid OrderId, Guid RequesterId, bool IsAdmin) : IRequest<OrderDto>;
