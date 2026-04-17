using AutoMapper;
using AutoMapper.QueryableExtensions;
using LocMp.Analytics.Application.DTOs;
using LocMp.Analytics.Domain.Enums;
using LocMp.Analytics.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Analytics.Application.Analytics.Queries.Seller;

public sealed record GetSellerSalesQuery(
    Guid SellerId,
    PeriodType PeriodType) : IRequest<SellerSalesSummaryDto?>;

public sealed class GetSellerSalesQueryHandler(AnalyticsDbContext db, IMapper mapper)
    : IRequestHandler<GetSellerSalesQuery, SellerSalesSummaryDto?>
{
    public async Task<SellerSalesSummaryDto?> Handle(GetSellerSalesQuery request, CancellationToken ct) =>
        await db.SellerSalesSummaries
            .Where(x => x.SellerId == request.SellerId && x.PeriodType == request.PeriodType)
            .OrderByDescending(x => x.PeriodStart)
            .ProjectTo<SellerSalesSummaryDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ct);
}
