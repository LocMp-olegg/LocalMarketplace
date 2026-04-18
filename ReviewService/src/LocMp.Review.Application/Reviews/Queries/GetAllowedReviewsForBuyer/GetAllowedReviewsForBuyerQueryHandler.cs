using LocMp.BuildingBlocks.Application.Common;
using LocMp.Review.Application.DTOs;
using LocMp.Review.Domain.Entities;
using LocMp.Review.Domain.Enums;
using LocMp.Review.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Review.Application.Reviews.Queries.GetAllowedReviewsForBuyer;

public sealed class GetAllowedReviewsForBuyerQueryHandler(ReviewDbContext db)
    : IRequestHandler<GetAllowedReviewsForBuyerQuery, PagedResult<PendingReviewSubjectDto>>
{
    public async Task<PagedResult<PendingReviewSubjectDto>> Handle(
        GetAllowedReviewsForBuyerQuery request, CancellationToken ct)
    {
        var allowedReviews = await db.AllowedReviews
            .Where(ar => ar.BuyerId == request.BuyerId)
            .OrderByDescending(ar => ar.AllowedAt)
            .ToListAsync(ct);

        if (allowedReviews.Count == 0)
            return new PagedResult<PendingReviewSubjectDto>([], 0, request.Page, request.PageSize);

        var reviewedSet = (await db.Reviews
                .Where(r => r.ReviewerId == request.BuyerId)
                .Select(r => new { r.SubjectType, r.SubjectId })
                .ToListAsync(ct))
            .Select(r => (r.SubjectType, r.SubjectId))
            .ToHashSet();

        var pending = new List<PendingReviewSubjectDto>();
        var seen = new HashSet<(ReviewSubjectType, Guid)>();

        foreach (var ar in allowedReviews)
        {
            if (ar.SellerId != Guid.Empty)
                TryAdd(ReviewSubjectType.Seller, ar.SellerId, ar);
            if (ar.CourierId.HasValue)
                TryAdd(ReviewSubjectType.Courier, ar.CourierId.Value, ar);
            foreach (var productId in ar.ProductIds)
                TryAdd(ReviewSubjectType.Product, productId, ar);
        }

        var total = pending.Count;
        var items = pending
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PagedResult<PendingReviewSubjectDto>(items, total, request.Page, request.PageSize);

        void TryAdd(ReviewSubjectType type, Guid subjectId, AllowedReview ar)
        {
            if (!reviewedSet.Contains((type, subjectId)) && seen.Add((type, subjectId)))
                pending.Add(new PendingReviewSubjectDto(ar.OrderId, type, subjectId, ar.AllowedAt));
        }
    }
}
