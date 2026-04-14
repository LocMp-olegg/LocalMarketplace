using AutoMapper;
using LocMp.BuildingBlocks.Application.Common;
using LocMp.Review.Application.DTOs;
using LocMp.Review.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Review.Application.Reviews.Queries.GetReviewsBySubject;

public sealed class GetReviewsBySubjectQueryHandler(ReviewDbContext db, IMapper mapper)
    : IRequestHandler<GetReviewsBySubjectQuery, PagedResult<ReviewSummaryDto>>
{
    public async Task<PagedResult<ReviewSummaryDto>> Handle(
        GetReviewsBySubjectQuery request, CancellationToken ct)
    {
        var query = db.Reviews
            .Include(r => r.Photos)
            .Include(r => r.Response)
            .Where(r => r.SubjectId == request.SubjectId && r.SubjectType == request.SubjectType);

        if (!request.IncludeHidden)
            query = query.Where(r => r.IsVisible);

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