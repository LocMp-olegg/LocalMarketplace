using LocMp.Order.Application.DTOs;
using MediatR;

namespace LocMp.Order.Application.Orders.Queries.Orders.GetCourierApplications;

public sealed record GetCourierApplicationsQuery(
    Guid SellerId,
    Guid OrderId) : IRequest<IReadOnlyList<CourierApplicationDto>>;