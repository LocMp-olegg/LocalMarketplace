using AutoMapper;
using AutoMapper.QueryableExtensions;
using LocMp.Analytics.Application.DTOs;
using LocMp.Analytics.Domain.Enums;
using LocMp.Analytics.Infrastructure.Consumers.Helpers;
using LocMp.Analytics.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Analytics.Application.Analytics.Queries.Admin;

public sealed record GetSellerLeaderboardQuery(
    PeriodType PeriodType,
    int Top = 10) : IRequest<List<SellerLeaderboardDto>>;

public sealed class GetSellerLeaderboardQueryHandler(AnalyticsDbContext db, IMapper mapper)
    : IRequestHandler<GetSellerLeaderboardQuery, List<SellerLeaderboardDto>>
{
    public async Task<List<SellerLeaderboardDto>> Handle(GetSellerLeaderboardQuery request, CancellationToken ct)
    {
        var periodStart = PeriodHelper.GetPeriodStart(request.PeriodType, DateTimeOffset.UtcNow);

        return await db.SellerLeaderboards
            .Where(x => x.PeriodType == request.PeriodType && x.PeriodStart == periodStart)
            .OrderBy(x => x.RevenueRank == 0 ? int.MaxValue : x.RevenueRank)
            .ThenByDescending(x => x.TotalRevenue)
            .Take(request.Top)
            .ProjectTo<SellerLeaderboardDto>(mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }
}
