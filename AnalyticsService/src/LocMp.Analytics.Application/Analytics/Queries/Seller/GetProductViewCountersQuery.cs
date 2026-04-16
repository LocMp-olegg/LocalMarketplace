using AutoMapper;
using AutoMapper.QueryableExtensions;
using LocMp.Analytics.Application.DTOs;
using LocMp.Analytics.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Analytics.Application.Analytics.Queries.Seller;

public sealed record GetProductViewCountersQuery(
    Guid SellerId,
    Guid? ProductId = null) : IRequest<List<ProductViewCounterDto>>;

public sealed class GetProductViewCountersQueryHandler(AnalyticsDbContext db, IMapper mapper)
    : IRequestHandler<GetProductViewCountersQuery, List<ProductViewCounterDto>>
{
    public async Task<List<ProductViewCounterDto>> Handle(GetProductViewCountersQuery request, CancellationToken ct)
    {
        var query = db.ProductViewCounters.Where(x => x.SellerId == request.SellerId);

        if (request.ProductId.HasValue)
            query = query.Where(x => x.ProductId == request.ProductId.Value);

        return await query
            .OrderByDescending(x => x.TotalViews)
            .ProjectTo<ProductViewCounterDto>(mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }
}
