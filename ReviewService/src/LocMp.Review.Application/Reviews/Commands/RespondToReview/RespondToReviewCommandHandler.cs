using AutoMapper;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Review.Application.DTOs;
using LocMp.Review.Domain.Entities;
using LocMp.Review.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Review.Application.Reviews.Commands.RespondToReview;

public sealed class RespondToReviewCommandHandler(ReviewDbContext db, IMapper mapper)
    : IRequestHandler<RespondToReviewCommand, ReviewResponseDto>
{
    public async Task<ReviewResponseDto> Handle(RespondToReviewCommand request, CancellationToken ct)
    {
        var review = await db.Reviews
                         .Include(r => r.Response)
                         .FirstOrDefaultAsync(r => r.Id == request.ReviewId, ct)
                     ?? throw new NotFoundException($"Review '{request.ReviewId}' not found.");

        var allowed = await db.AllowedReviews
            .FirstOrDefaultAsync(ar => ar.OrderId == review.OrderId, ct);

        if (allowed is null || allowed.SellerId != request.AuthorId)
            throw new ForbiddenException("Only the seller of this order can respond to the review.");

        if (review.Response is not null)
            throw new ConflictException("A response to this review already exists.");

        var response = new ReviewResponse(Guid.NewGuid())
        {
            ReviewId = review.Id,
            AuthorId = request.AuthorId,
            Comment = request.Comment,
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.ReviewResponses.Add(response);
        await db.SaveChangesAsync(ct);

        return mapper.Map<ReviewResponseDto>(response);
    }
}