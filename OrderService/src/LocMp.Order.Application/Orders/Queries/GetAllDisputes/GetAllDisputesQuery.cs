using LocMp.BuildingBlocks.Application.Common;
using LocMp.Order.Application.DTOs;
using LocMp.Order.Domain.Enums;
using MediatR;

namespace LocMp.Order.Application.Orders.Queries.GetAllDisputes;

public sealed record GetAllDisputesQuery(
    DisputeStatus? StatusFilter,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<DisputeSummaryDto>>;
