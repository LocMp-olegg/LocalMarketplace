using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Catalog.Application.Catalog.Commands.Shops.DeleteShopPhoto;

public sealed class DeleteShopPhotoCommandHandler(CatalogDbContext db, IStorageService storageService)
    : IRequestHandler<DeleteShopPhotoCommand>
{
    public async Task Handle(DeleteShopPhotoCommand request, CancellationToken ct)
    {
        var photo = await db.ShopPhotos
                        .Include(p => p.Shop)
                        .FirstOrDefaultAsync(p => p.Id == request.PhotoId, ct)
                    ?? throw new NotFoundException($"Photo '{request.PhotoId}' not found.");

        if (!request.IsAdmin && photo.Shop.SellerId != request.RequesterId)
            throw new ForbiddenException("You can only delete photos from your own shop.");

        await storageService.DeleteAsync(photo.ObjectKey, ct);
        db.ShopPhotos.Remove(photo);
        await db.SaveChangesAsync(ct);
    }
}
