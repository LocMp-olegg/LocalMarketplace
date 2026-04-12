using LocMp.BuildingBlocks.Application.Common;
using LocMp.Order.Application.DTOs;
using LocMp.Order.Domain.Enums;
using MediatR;

namespace LocMp.Order.Application.Orders.Queries.GetOrdersAssignedToCourier;

public sealed record GetOrdersAssignedToCourierQuery(
    Guid CourierId,
    OrderStatus? StatusFilter,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<OrderSummaryDto>>;
