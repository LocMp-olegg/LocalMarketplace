using LocMp.Order.Application.DTOs;
using MediatR;

namespace LocMp.Order.Application.Orders.Queries.GetAvailableOrdersForCourier;

public sealed record GetAvailableOrdersForCourierQuery(
    double Latitude,
    double Longitude,
    double RadiusKm) : IRequest<IReadOnlyList<OrderSummaryDto>>;
