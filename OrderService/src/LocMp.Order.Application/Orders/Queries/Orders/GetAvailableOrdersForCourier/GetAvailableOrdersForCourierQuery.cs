using LocMp.BuildingBlocks.Application.Common;
using LocMp.Order.Application.DTOs;
using MediatR;

namespace LocMp.Order.Application.Orders.Queries.Orders.GetAvailableOrdersForCourier;

public sealed record GetAvailableOrdersForCourierQuery(
    double Latitude,
    double Longitude,
    double RadiusKm,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<OrderSummaryDto>>;