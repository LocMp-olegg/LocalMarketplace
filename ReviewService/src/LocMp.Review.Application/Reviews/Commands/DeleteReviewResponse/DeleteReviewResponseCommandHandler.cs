using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Review.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Review.Application.Reviews.Commands.DeleteReviewResponse;

public sealed class DeleteReviewResponseCommandHandler(ReviewDbContext db)
    : IRequestHandler<DeleteReviewResponseCommand>
{
    public async Task Handle(DeleteReviewResponseCommand request, CancellationToken ct)
    {
        var review = await db.Reviews
                         .Include(r => r.Response)
                         .FirstOrDefaultAsync(r => r.Id == request.ReviewId, ct)
                     ?? throw new NotFoundException($"Review '{request.ReviewId}' not found.");

        if (review.Response is null)
            throw new NotFoundException($"Response for review '{request.ReviewId}' not found.");

        if (!request.IsAdmin && review.Response.AuthorId != request.RequesterId)
            throw new ForbiddenException("You are not the author of this response.");

        db.ReviewResponses.Remove(review.Response);
        await db.SaveChangesAsync(ct);
    }
}
