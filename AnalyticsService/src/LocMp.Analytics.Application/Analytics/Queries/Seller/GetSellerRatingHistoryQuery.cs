using AutoMapper;
using AutoMapper.QueryableExtensions;
using LocMp.Analytics.Application.DTOs;
using LocMp.Analytics.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Analytics.Application.Analytics.Queries.Seller;

public sealed record GetSellerRatingHistoryQuery(
    Guid SellerId,
    int Days = 30) : IRequest<List<SellerRatingHistoryDto>>;

public sealed class GetSellerRatingHistoryQueryHandler(AnalyticsDbContext db, IMapper mapper)
    : IRequestHandler<GetSellerRatingHistoryQuery, List<SellerRatingHistoryDto>>
{
    public async Task<List<SellerRatingHistoryDto>> Handle(GetSellerRatingHistoryQuery request, CancellationToken ct)
    {
        var from = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-request.Days));

        return await db.SellerRatingHistory
            .Where(x => x.SellerId == request.SellerId && x.RecordedAt >= from)
            .OrderBy(x => x.RecordedAt)
            .ProjectTo<SellerRatingHistoryDto>(mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }
}
