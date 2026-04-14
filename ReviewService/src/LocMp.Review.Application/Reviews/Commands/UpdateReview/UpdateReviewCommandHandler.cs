using AutoMapper;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Contracts.Review;
using LocMp.Review.Application.DTOs;
using LocMp.Review.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace LocMp.Review.Application.Reviews.Commands.UpdateReview;

public sealed class UpdateReviewCommandHandler(
    ReviewDbContext db,
    IMapper mapper,
    IEventBus eventBus,
    IDistributedCache cache)
    : IRequestHandler<UpdateReviewCommand, ReviewDto>
{
    public async Task<ReviewDto> Handle(UpdateReviewCommand request, CancellationToken ct)
    {
        var review = await db.Reviews
                         .Include(r => r.Photos)
                         .Include(r => r.Response)
                         .FirstOrDefaultAsync(r => r.Id == request.ReviewId, ct)
                     ?? throw new NotFoundException($"Review '{request.ReviewId}' not found.");

        if (review.ReviewerId != request.RequesterId)
            throw new ForbiddenException("You are not the author of this review.");

        var now = DateTimeOffset.UtcNow;
        var oldRating = review.Rating;
        var ratingChanged = review.Rating != request.Rating;

        review.Rating = request.Rating;
        review.Comment = request.Comment;
        review.UpdatedAt = now;

        if (ratingChanged)
        {
            var ratingAggregate = await db.RatingAggregates
                .FirstOrDefaultAsync(ra =>
                    ra.SubjectId == review.SubjectId && ra.SubjectType == review.SubjectType, ct);

            if (ratingAggregate is not null)
            {
                ratingAggregate.RemoveRating(oldRating, now);
                ratingAggregate.AddRating(request.Rating, now);

                await db.SaveChangesAsync(ct);

                await cache.RemoveAsync($"rating:{review.SubjectType}:{review.SubjectId}", ct);

                await eventBus.PublishAsync(new RatingAggregateUpdatedEvent(
                    review.SubjectId,
                    review.SubjectType.ToString(),
                    ratingAggregate.AverageRating,
                    ratingAggregate.ReviewCount,
                    now), ct);
            }
            else
            {
                await db.SaveChangesAsync(ct);
            }
        }
        else
        {
            await db.SaveChangesAsync(ct);
        }

        return mapper.Map<ReviewDto>(review);
    }
}