using AutoMapper;
using AutoMapper.QueryableExtensions;
using LocMp.Analytics.Application.DTOs;
using LocMp.Analytics.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Analytics.Application.Analytics.Queries.Seller;

public sealed record GetSellerProductRatingsQuery(Guid SellerId) : IRequest<SellerProductRatingsDto>;

public sealed class GetSellerProductRatingsQueryHandler(AnalyticsDbContext db, IMapper mapper)
    : IRequestHandler<GetSellerProductRatingsQuery, SellerProductRatingsDto>
{
    public async Task<SellerProductRatingsDto> Handle(GetSellerProductRatingsQuery request, CancellationToken ct)
    {
        var products = await db.ProductRatingSummaries
            .Where(x => x.SellerId == request.SellerId && x.ReviewCount > 0)
            .OrderByDescending(x => x.AverageRating)
            .ProjectTo<ProductRatingSummaryDto>(mapper.ConfigurationProvider)
            .ToListAsync(ct);

        var totalReviews = products.Sum(p => p.ReviewCount);
        var overallAverage = totalReviews > 0
            ? products.Sum(p => p.AverageRating * p.ReviewCount) / totalReviews
            : 0m;

        return new SellerProductRatingsDto(products, Math.Round(overallAverage, 2), totalReviews);
    }
}
