using LocMp.BuildingBlocks.Application.Common;
using LocMp.Review.Application.DTOs;
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

        var orderIds = allowedReviews.Select(ar => ar.OrderId).ToList();

        // Load all subjects already reviewed for these orders
        var reviewedKeys = await db.Reviews
            .Where(r => orderIds.Contains(r.OrderId))
            .Select(r => new { r.OrderId, r.SubjectType, r.SubjectId })
            .ToListAsync(ct);

        var reviewedSet = reviewedKeys
            .Select(r => (r.OrderId, r.SubjectType, r.SubjectId))
            .ToHashSet();

        // Expand each AllowedReview into individual subjects, filter out already reviewed
        var pending = new List<PendingReviewSubjectDto>();
        foreach (var ar in allowedReviews)
        {
            if (!reviewedSet.Contains((ar.OrderId, ReviewSubjectType.Seller, ar.SellerId)))
                pending.Add(new PendingReviewSubjectDto(ar.OrderId, ReviewSubjectType.Seller, ar.SellerId, ar.AllowedAt));

            if (ar.CourierId.HasValue &&
                !reviewedSet.Contains((ar.OrderId, ReviewSubjectType.Courier, ar.CourierId.Value)))
                pending.Add(new PendingReviewSubjectDto(ar.OrderId, ReviewSubjectType.Courier, ar.CourierId.Value, ar.AllowedAt));

            foreach (var productId in ar.ProductIds)
            {
                if (!reviewedSet.Contains((ar.OrderId, ReviewSubjectType.Product, productId)))
                    pending.Add(new PendingReviewSubjectDto(ar.OrderId, ReviewSubjectType.Product, productId, ar.AllowedAt));
            }
        }

        var total = pending.Count;
        var items = pending
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PagedResult<PendingReviewSubjectDto>(items, total, request.Page, request.PageSize);
    }
}
