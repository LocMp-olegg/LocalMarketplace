using AutoMapper;
using AutoMapper.QueryableExtensions;
using LocMp.Analytics.Application.DTOs;
using LocMp.Analytics.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Analytics.Application.Analytics.Queries.Admin;

public sealed record GetDisputeSummaryQuery(
    DateOnly? From = null,
    DateOnly? To   = null) : IRequest<List<DisputeSummaryDto>>;

public sealed class GetDisputeSummaryQueryHandler(AnalyticsDbContext db, IMapper mapper)
    : IRequestHandler<GetDisputeSummaryQuery, List<DisputeSummaryDto>>
{
    public async Task<List<DisputeSummaryDto>> Handle(GetDisputeSummaryQuery request, CancellationToken ct)
    {
        var query = db.DisputeSummaries.AsQueryable();

        if (request.From.HasValue) query = query.Where(x => x.Date >= request.From.Value);
        if (request.To.HasValue)   query = query.Where(x => x.Date <= request.To.Value);

        return await query
            .OrderBy(x => x.Date)
            .ProjectTo<DisputeSummaryDto>(mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }
}
