using AutoMapper;
using AutoMapper.QueryableExtensions;
using LocMp.Analytics.Application.DTOs;
using LocMp.Analytics.Domain.Enums;
using LocMp.Analytics.Infrastructure.Consumers.Helpers;
using LocMp.Analytics.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Analytics.Application.Analytics.Queries.Seller;

public sealed record GetSellerTopProductsQuery(
    Guid SellerId,
    PeriodType PeriodType,
    int Top = 10) : IRequest<List<TopProductDto>>;

public sealed class GetSellerTopProductsQueryHandler(AnalyticsDbContext db, IMapper mapper)
    : IRequestHandler<GetSellerTopProductsQuery, List<TopProductDto>>
{
    public async Task<List<TopProductDto>> Handle(GetSellerTopProductsQuery request, CancellationToken ct)
    {
        var periodStart = PeriodHelper.GetPeriodStart(request.PeriodType, DateTimeOffset.UtcNow);

        return await db.TopProducts
            .Where(x => x.SellerId == request.SellerId
                     && x.PeriodType == request.PeriodType
                     && x.PeriodStart == periodStart)
            .OrderByDescending(x => x.TotalSold)
            .ThenByDescending(x => x.TotalRevenue)
            .Take(request.Top)
            .ProjectTo<TopProductDto>(mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }
}
