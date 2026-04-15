using AutoMapper;
using LocMp.BuildingBlocks.Application.Common;
using LocMp.Review.Application.DTOs;
using LocMp.Review.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Review.Application.Reviews.Queries.GetReviewsForModeration;

public sealed class GetReviewsForModerationQueryHandler(ReviewDbContext db, IMapper mapper)
    : IRequestHandler<GetReviewsForModerationQuery, PagedResult<ReviewSummaryDto>>
{
    public async Task<PagedResult<ReviewSummaryDto>> Handle(
        GetReviewsForModerationQuery request, CancellationToken ct)
    {
        var query = db.Reviews
            .Include(r => r.Photos)
            .Include(r => r.Response)
            .AsQueryable();

        if (request.IsVisible.HasValue)
            query = query.Where(r => r.IsVisible == request.IsVisible.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<ReviewSummaryDto>(
            mapper.Map<List<ReviewSummaryDto>>(items),
            total, request.Page, request.PageSize);
    }
}
