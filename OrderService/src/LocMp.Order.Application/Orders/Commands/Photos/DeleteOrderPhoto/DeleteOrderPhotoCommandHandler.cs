using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Order.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Order.Application.Orders.Commands.Photos.DeleteOrderPhoto;

public sealed class DeleteOrderPhotoCommandHandler(OrderDbContext db, IStorageService storageService)
    : IRequestHandler<DeleteOrderPhotoCommand>
{
    public async Task Handle(DeleteOrderPhotoCommand request, CancellationToken ct)
    {
        var photo = await db.OrderPhotos
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.Id == request.PhotoId, ct)
            ?? throw new NotFoundException($"Photo '{request.PhotoId}' not found.");

        if (!request.IsAdmin
            && photo.Order.BuyerId != request.RequesterId
            && photo.Order.SellerId != request.RequesterId)
            throw new ForbiddenException("You are not a participant in this order.");

        await storageService.DeleteAsync(photo.ObjectKey, ct);
        db.OrderPhotos.Remove(photo);
        await db.SaveChangesAsync(ct);
    }
}
