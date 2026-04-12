using LocMp.Order.Application.DTOs;
using MediatR;

namespace LocMp.Order.Application.Orders.Queries.GetDisputeById;

public sealed record GetDisputeByIdQuery(Guid DisputeId, Guid RequesterId, bool IsAdmin) : IRequest<DisputeDto>;
