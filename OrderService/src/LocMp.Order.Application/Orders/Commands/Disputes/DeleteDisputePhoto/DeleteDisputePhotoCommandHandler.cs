using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Order.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Order.Application.Orders.Commands.Disputes.DeleteDisputePhoto;

public sealed class DeleteDisputePhotoCommandHandler(OrderDbContext db, IStorageService storageService)
    : IRequestHandler<DeleteDisputePhotoCommand>
{
    public async Task Handle(DeleteDisputePhotoCommand request, CancellationToken ct)
    {
        var photo = await db.DisputePhotos
                        .FirstOrDefaultAsync(p => p.Id == request.PhotoId, ct)
                    ?? throw new NotFoundException($"Dispute photo '{request.PhotoId}' not found.");

        if (!request.IsAdmin && photo.UploadedById != request.RequesterId)
            throw new ForbiddenException("Only the photo uploader can delete it.");

        await storageService.DeleteAsync(photo.ObjectKey, ct);
        db.DisputePhotos.Remove(photo);
        await db.SaveChangesAsync(ct);
    }
}