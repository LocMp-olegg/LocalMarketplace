using LocMp.BuildingBlocks.Application.Common;
using LocMp.Order.Application.DTOs;
using LocMp.Order.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Order.Application.Orders.Queries.Disputes.GetAllDisputes;

public sealed class GetAllDisputesQueryHandler(OrderDbContext db)
    : IRequestHandler<GetAllDisputesQuery, PagedResult<DisputeSummaryDto>>
{
    public async Task<PagedResult<DisputeSummaryDto>> Handle(GetAllDisputesQuery request, CancellationToken ct)
    {
        var query = db.Disputes.AsQueryable();

        if (request.StatusFilter.HasValue)
            query = query.Where(d => d.Status == request.StatusFilter.Value);

        var total = await query.CountAsync(ct);

        var disputes = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var items = disputes.Select(d => new DisputeSummaryDto(
            d.Id, d.OrderId, d.InitiatorId,
            d.Reason, d.Status, d.Outcome,
            d.CreatedAt, d.ResolvedAt)).ToList();

        return new PagedResult<DisputeSummaryDto>(items, total, request.PageNumber, request.PageSize);
    }
}