using AutoMapper;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Review.Application.DTOs;
using LocMp.Review.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Review.Application.Reviews.Commands.UpdateReviewResponse;

public sealed class UpdateReviewResponseCommandHandler(ReviewDbContext db, IMapper mapper)
    : IRequestHandler<UpdateReviewResponseCommand, ReviewResponseDto>
{
    public async Task<ReviewResponseDto> Handle(UpdateReviewResponseCommand request, CancellationToken ct)
    {
        var review = await db.Reviews
                         .Include(r => r.Response)
                         .FirstOrDefaultAsync(r => r.Id == request.ReviewId, ct)
                     ?? throw new NotFoundException($"Review '{request.ReviewId}' not found.");

        if (review.Response is null)
            throw new NotFoundException($"Response for review '{request.ReviewId}' not found.");

        if (review.Response.AuthorId != request.AuthorId)
            throw new ForbiddenException("You are not the author of this response.");

        review.Response.Comment = request.Comment;
        review.Response.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);

        return mapper.Map<ReviewResponseDto>(review.Response);
    }
}
