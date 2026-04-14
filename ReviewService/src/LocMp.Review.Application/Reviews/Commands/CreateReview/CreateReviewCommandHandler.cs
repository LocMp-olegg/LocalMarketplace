using AutoMapper;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Contracts.Review;
using LocMp.Review.Application.DTOs;
using LocMp.Review.Domain.Entities;
using LocMp.Review.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using ReviewEntity = LocMp.Review.Domain.Entities.Review;

namespace LocMp.Review.Application.Reviews.Commands.CreateReview;

public sealed class CreateReviewCommandHandler(
    ReviewDbContext db,
    IMapper mapper,
    IEventBus eventBus,
    IDistributedCache cache)
    : IRequestHandler<CreateReviewCommand, ReviewDto>
{
    public async Task<ReviewDto> Handle(CreateReviewCommand request, CancellationToken ct)
    {
        var allowed = await db.AllowedReviews
                          .FirstOrDefaultAsync(ar => ar.OrderId == request.OrderId, ct)
                      ?? throw new ConflictException("This order cannot be reviewed. It may not be completed yet.");

        if (allowed.BuyerId != request.ReviewerId)
            throw new ForbiddenException("Only the buyer of this order can leave a review.");

        var alreadyExists = await db.Reviews.AnyAsync(r => r.OrderId == request.OrderId, ct);
        if (alreadyExists)
            throw new ConflictException("A review for this order already exists.");

        var now = DateTimeOffset.UtcNow;
        var review = new ReviewEntity(Guid.NewGuid())
        {
            OrderId = request.OrderId,
            ReviewerId = request.ReviewerId,
            ReviewerName = request.ReviewerName,
            SubjectType = request.SubjectType,
            SubjectId = request.SubjectId,
            Rating = request.Rating,
            Comment = request.Comment,
            CreatedAt = now
        };

        db.Reviews.Add(review);

        var ratingAggregate = await db.RatingAggregates
            .FirstOrDefaultAsync(ra =>
                ra.SubjectId == request.SubjectId && ra.SubjectType == request.SubjectType, ct);

        if (ratingAggregate is null)
        {
            ratingAggregate = new RatingAggregate
            {
                SubjectId = request.SubjectId,
                SubjectType = request.SubjectType
            };
            db.RatingAggregates.Add(ratingAggregate);
        }

        ratingAggregate.AddRating(request.Rating, now);

        await db.SaveChangesAsync(ct);

        await cache.RemoveAsync($"rating:{request.SubjectType}:{request.SubjectId}", ct);

        await eventBus.PublishAsync(new RatingAggregateUpdatedEvent(
            request.SubjectId,
            request.SubjectType.ToString(),
            ratingAggregate.AverageRating,
            ratingAggregate.ReviewCount,
            now), ct);

        return mapper.Map<ReviewDto>(review);
    }
}