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

public sealed class GetSellerLeaderboardQueryHandler(AnalyticsDbContext db)
    : IRequestHandler<GetSellerLeaderboardQuery, List<SellerLeaderboardDto>>
{
    public async Task<List<SellerLeaderboardDto>> Handle(GetSellerLeaderboardQuery request, CancellationToken ct)
    {
        var periodStart = PeriodHelper.GetPeriodStart(request.PeriodType, DateTimeOffset.UtcNow);

        var all = await db.SellerLeaderboards
            .Where(x => x.PeriodType == request.PeriodType && x.PeriodStart == periodStart)
            .ToListAsync(ct);

        var sellerRows = all
            .Where(x => x.ShopId == null)
            .OrderBy(x => x.RevenueRank == 0 ? int.MaxValue : x.RevenueRank)
            .ThenByDescending(x => x.TotalRevenue)
            .Take(request.Top)
            .ToList();

        var sellerIds = sellerRows.Select(x => x.SellerId).ToHashSet();

        var shopsBySeller = all
            .Where(x => x.ShopId != null && sellerIds.Contains(x.SellerId))
            .GroupBy(x => x.SellerId)
            .ToDictionary(g => g.Key, g => g.ToList());

        return sellerRows.Select(row =>
        {
            var shops = shopsBySeller.TryGetValue(row.SellerId, out var shopRows)
                ? shopRows
                    .OrderByDescending(s => s.TotalRevenue)
                    .Select(s => new ShopLeaderboardDto(s.ShopId!.Value, s.ShopName!, s.TotalRevenue, s.OrderCount))
                    .ToList()
                : (IReadOnlyList<ShopLeaderboardDto>)[];

            return new SellerLeaderboardDto(
                row.SellerId, row.SellerName,
                row.PeriodType, row.PeriodStart,
                row.TotalRevenue, row.OrderCount, row.AverageRating,
                row.RevenueRank, row.OrderCountRank, row.RanksComputedAt,
                shops);
        }).ToList();
    }
}
