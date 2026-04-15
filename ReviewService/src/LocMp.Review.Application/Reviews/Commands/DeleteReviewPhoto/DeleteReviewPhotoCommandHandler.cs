using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Review.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Review.Application.Reviews.Commands.DeleteReviewPhoto;

public sealed class DeleteReviewPhotoCommandHandler(ReviewDbContext db, IStorageService storage)
    : IRequestHandler<DeleteReviewPhotoCommand>
{
    public async Task Handle(DeleteReviewPhotoCommand request, CancellationToken ct)
    {
        var photo = await db.ReviewPhotos
                        .Include(p => p.Review)
                        .FirstOrDefaultAsync(p => p.Id == request.PhotoId, ct)
                    ?? throw new NotFoundException($"ReviewPhoto '{request.PhotoId}' not found.");

        if (!request.IsAdmin && photo.Review.ReviewerId != request.RequesterId)
            throw new ForbiddenException("You are not the author of this review.");

        await storage.DeleteAsync(photo.ObjectKey, ct);
        db.ReviewPhotos.Remove(photo);
        await db.SaveChangesAsync(ct);
    }
}