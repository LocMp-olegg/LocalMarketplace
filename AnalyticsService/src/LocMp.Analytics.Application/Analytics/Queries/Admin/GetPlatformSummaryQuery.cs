using AutoMapper;
using AutoMapper.QueryableExtensions;
using LocMp.Analytics.Application.DTOs;
using LocMp.Analytics.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Analytics.Application.Analytics.Queries.Admin;

public sealed record GetPlatformSummaryQuery(
    DateOnly From,
    DateOnly To) : IRequest<List<PlatformDailySummaryDto>>;

public sealed class GetPlatformSummaryQueryHandler(AnalyticsDbContext db, IMapper mapper)
    : IRequestHandler<GetPlatformSummaryQuery, List<PlatformDailySummaryDto>>
{
    public async Task<List<PlatformDailySummaryDto>> Handle(GetPlatformSummaryQuery request, CancellationToken ct) =>
        await db.PlatformDailySummaries
            .Where(x => x.Date >= request.From && x.Date <= request.To)
            .OrderBy(x => x.Date)
            .ProjectTo<PlatformDailySummaryDto>(mapper.ConfigurationProvider)
            .ToListAsync(ct);
}
