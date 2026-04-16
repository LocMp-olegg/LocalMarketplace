using AutoMapper;
using AutoMapper.QueryableExtensions;
using LocMp.Analytics.Application.DTOs;
using LocMp.Analytics.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Analytics.Application.Analytics.Queries.Seller;

public sealed record GetSellerStockAlertsQuery(
    Guid SellerId,
    bool OnlyUnacknowledged = false) : IRequest<List<StockAlertDto>>;

public sealed class GetSellerStockAlertsQueryHandler(AnalyticsDbContext db, IMapper mapper)
    : IRequestHandler<GetSellerStockAlertsQuery, List<StockAlertDto>>
{
    public async Task<List<StockAlertDto>> Handle(GetSellerStockAlertsQuery request, CancellationToken ct)
    {
        var query = db.StockAlerts.Where(x => x.SellerId == request.SellerId);

        if (request.OnlyUnacknowledged)
            query = query.Where(x => !x.IsAcknowledged);

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .ProjectTo<StockAlertDto>(mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }
}
