using AutoMapper;
using AutoMapper.QueryableExtensions;
using LocMp.Analytics.Application.DTOs;
using LocMp.Analytics.Domain.Enums;
using LocMp.Analytics.Infrastructure.Consumers.Helpers;
using LocMp.Analytics.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Analytics.Application.Analytics.Queries.Admin;

public sealed record GetGeographicActivityQuery(
    PeriodType PeriodType) : IRequest<List<GeographicActivityDto>>;

public sealed class GetGeographicActivityQueryHandler(AnalyticsDbContext db, IMapper mapper)
    : IRequestHandler<GetGeographicActivityQuery, List<GeographicActivityDto>>
{
    public async Task<List<GeographicActivityDto>> Handle(GetGeographicActivityQuery request, CancellationToken ct)
    {
        var periodStart = PeriodHelper.GetPeriodStart(request.PeriodType, DateTimeOffset.UtcNow);

        return await db.GeographicActivities
            .Where(x => x.PeriodType == request.PeriodType && x.PeriodStart == periodStart)
            .OrderByDescending(x => x.TotalOrders)
            .ProjectTo<GeographicActivityDto>(mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }
}
