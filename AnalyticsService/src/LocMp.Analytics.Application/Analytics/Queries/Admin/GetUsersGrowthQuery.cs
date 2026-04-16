using LocMp.Analytics.Application.DTOs;
using LocMp.Analytics.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Analytics.Application.Analytics.Queries.Admin;

public sealed record GetUsersGrowthQuery(
    DateOnly From,
    DateOnly To) : IRequest<List<UserGrowthDto>>;

public sealed class GetUsersGrowthQueryHandler(AnalyticsDbContext db)
    : IRequestHandler<GetUsersGrowthQuery, List<UserGrowthDto>>
{
    public async Task<List<UserGrowthDto>> Handle(GetUsersGrowthQuery request, CancellationToken ct) =>
        await db.PlatformDailySummaries
            .Where(x => x.Date >= request.From && x.Date <= request.To)
            .OrderBy(x => x.Date)
            .Select(x => new UserGrowthDto(x.Date, x.NewRegistrations, x.ActiveBuyers, x.ActiveSellers, x.BlockedUsers))
            .ToListAsync(ct);
}
