using LocMp.BuildingBlocks.Application.Common;
using LocMp.Order.Application.DTOs;
using LocMp.Order.Domain.Enums;
using MediatR;

namespace LocMp.Order.Application.Orders.Queries.Orders.GetMyCourierApplications;

public sealed record GetMyCourierApplicationsQuery(
    Guid CourierId,
    CourierApplicationStatus? Status,
    int PageNumber,
    int PageSize) : IRequest<PagedResult<CourierApplicationDto>>;