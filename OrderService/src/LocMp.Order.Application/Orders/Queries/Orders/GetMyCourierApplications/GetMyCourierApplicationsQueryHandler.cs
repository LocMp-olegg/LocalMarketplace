using AutoMapper;
using LocMp.BuildingBlocks.Application.Common;
using LocMp.Order.Application.DTOs;
using LocMp.Order.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Order.Application.Orders.Queries.Orders.GetMyCourierApplications;

public sealed class GetMyCourierApplicationsQueryHandler(OrderDbContext db, IMapper mapper)
    : IRequestHandler<GetMyCourierApplicationsQuery, PagedResult<CourierApplicationDto>>
{
    public async Task<PagedResult<CourierApplicationDto>> Handle(
        GetMyCourierApplicationsQuery request, CancellationToken ct)
    {
        var query = db.CourierApplications
            .AsNoTracking()
            .Where(a => a.CourierId == request.CourierId);

        if (request.Status.HasValue)
            query = query.Where(a => a.Status == request.Status.Value);

        var total = await query.CountAsync(ct);

        var applications = await query
            .OrderByDescending(a => a.AppliedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var items = applications.Select(mapper.Map<CourierApplicationDto>).ToList();
        return new PagedResult<CourierApplicationDto>(items, total, request.PageNumber, request.PageSize);
    }
}