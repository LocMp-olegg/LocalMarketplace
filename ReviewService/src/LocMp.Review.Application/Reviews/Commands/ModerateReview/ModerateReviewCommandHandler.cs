using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Review.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Review.Application.Reviews.Commands.ModerateReview;

public sealed class ModerateReviewCommandHandler(ReviewDbContext db)
    : IRequestHandler<ModerateReviewCommand>
{
    public async Task Handle(ModerateReviewCommand request, CancellationToken ct)
    {
        var review = await db.Reviews.FirstOrDefaultAsync(r => r.Id == request.ReviewId, ct)
                     ?? throw new NotFoundException($"Review '{request.ReviewId}' not found.");

        var now = DateTimeOffset.UtcNow;
        if (request.IsVisible)
            review.Restore(now);
        else
            review.Hide(now);

        await db.SaveChangesAsync(ct);
    }
}