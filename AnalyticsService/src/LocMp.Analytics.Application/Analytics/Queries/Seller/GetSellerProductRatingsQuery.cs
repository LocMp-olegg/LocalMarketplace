using LocMp.Analytics.Application.DTOs;
using LocMp.Analytics.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Analytics.Application.Analytics.Queries.Seller;

public sealed record GetSellerProductRatingsQuery(Guid SellerId) : IRequest<SellerProductRatingsDto>;

public sealed class GetSellerProductRatingsQueryHandler(AnalyticsDbContext db)
    : IRequestHandler<GetSellerProductRatingsQuery, SellerProductRatingsDto>
{
    public async Task<SellerProductRatingsDto> Handle(GetSellerProductRatingsQuery request, CancellationToken ct)
    {
        var products = await db.ProductRatingSummaries
            .Where(x => x.SellerId == request.SellerId)
            .ToListAsync(ct);

        var shops = products
            .GroupBy(x => new { x.ShopId, x.ShopName })
            .Select(g =>
            {
                var items = g.OrderByDescending(p => p.AverageRating)
                    .Select(p => new ProductRatingSummaryDto(p.ProductId, p.ProductName, p.AverageRating, p.ReviewCount))
                    .ToList();

                var shopTotal = g.Sum(p => p.ReviewCount);
                var shopAvg = shopTotal > 0
                    ? Math.Round(g.Sum(p => p.AverageRating * p.ReviewCount) / shopTotal, 2)
                    : 0m;

                return new ShopRatingsDto(g.Key.ShopId, g.Key.ShopName, shopAvg, shopTotal, items);
            })
            .OrderByDescending(s => s.ShopReviewCount)
            .ToList();

        var totalReviews = products.Sum(p => p.ReviewCount);
        var overallAvg = totalReviews > 0
            ? Math.Round(products.Sum(p => p.AverageRating * p.ReviewCount) / totalReviews, 2)
            : 0m;

        return new SellerProductRatingsDto(overallAvg, totalReviews, shops);
    }
}
