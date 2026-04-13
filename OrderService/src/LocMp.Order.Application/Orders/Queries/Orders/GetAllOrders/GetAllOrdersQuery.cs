using LocMp.BuildingBlocks.Application.Common;
using LocMp.Order.Application.DTOs;
using LocMp.Order.Domain.Enums;
using MediatR;

namespace LocMp.Order.Application.Orders.Queries.Orders.GetAllOrders;

public sealed record GetAllOrdersQuery(
    OrderStatus? StatusFilter,
    DateTimeOffset? From,
    DateTimeOffset? To,
    decimal? MinAmount,
    decimal? MaxAmount,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<OrderSummaryDto>>;