using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Contracts.Review;
using LocMp.Review.Domain.Enums;
using LocMp.Review.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace LocMp.Review.Application.Reviews.Commands.DeleteReview;

public sealed class DeleteReviewCommandHandler(
    ReviewDbContext db,
    IEventBus eventBus,
    IDistributedCache cache)
    : IRequestHandler<DeleteReviewCommand>
{
    public async Task Handle(DeleteReviewCommand request, CancellationToken ct)
    {
        var review = await db.Reviews
                         .FirstOrDefaultAsync(r => r.Id == request.ReviewId, ct)
                     ?? throw new NotFoundException($"Review '{request.ReviewId}' not found.");

        if (!request.IsAdmin && review.ReviewerId != request.RequesterId)
            throw new ForbiddenException("You are not the author of this review.");

        var now = DateTimeOffset.UtcNow;
        var oldRating = review.Rating;
        var subjectId = review.SubjectId;
        var subjectType = review.SubjectType;

        db.Reviews.Remove(review);

        var ratingAggregate = await db.RatingAggregates
            .FirstOrDefaultAsync(ra => ra.SubjectId == subjectId && ra.SubjectType == subjectType, ct);

        if (ratingAggregate is not null)
            ratingAggregate.RemoveRating(oldRating, now);

        await db.SaveChangesAsync(ct);

        await cache.RemoveAsync($"rating:{subjectType}:{subjectId}", ct);

        if (ratingAggregate is not null)
        {
            await eventBus.PublishAsync(new RatingAggregateUpdatedEvent(
                subjectId,
                subjectType.ToString(),
                ratingAggregate.AverageRating,
                ratingAggregate.ReviewCount,
                now), ct);
        }
    }
}