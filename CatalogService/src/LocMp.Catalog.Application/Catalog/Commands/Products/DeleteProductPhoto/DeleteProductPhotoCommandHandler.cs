using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace LocMp.Catalog.Application.Catalog.Commands.Products.DeleteProductPhoto;

public sealed class DeleteProductPhotoCommandHandler(
    CatalogDbContext db,
    IStorageService storageService,
    IDistributedCache cache)
    : IRequestHandler<DeleteProductPhotoCommand>
{
    public async Task Handle(DeleteProductPhotoCommand request, CancellationToken ct)
    {
        var photo = await db.ProductPhotos
            .Include(p => p.Product)
            .FirstOrDefaultAsync(p => p.Id == request.PhotoId, ct)
            ?? throw new NotFoundException($"Photo '{request.PhotoId}' not found.");

        if (!request.IsAdmin && photo.Product.SellerId != request.RequesterId)
            throw new ForbiddenException("You do not own this product.");

        await storageService.DeleteAsync(photo.ObjectKey, ct);
        db.ProductPhotos.Remove(photo);
        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync($"product:{photo.ProductId}", ct);
    }
}
