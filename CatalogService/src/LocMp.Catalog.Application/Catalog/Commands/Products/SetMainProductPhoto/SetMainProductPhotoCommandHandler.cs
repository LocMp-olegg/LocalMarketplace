using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace LocMp.Catalog.Application.Catalog.Commands.Products.SetMainProductPhoto;

public sealed class SetMainProductPhotoCommandHandler(CatalogDbContext db, IDistributedCache cache)
    : IRequestHandler<SetMainProductPhotoCommand>
{
    public async Task Handle(SetMainProductPhotoCommand request, CancellationToken ct)
    {
        var photo = await db.ProductPhotos
                        .Include(p => p.Product)
                        .FirstOrDefaultAsync(p => p.Id == request.PhotoId, ct)
                    ?? throw new NotFoundException($"Photo '{request.PhotoId}' not found.");

        if (photo.ProductId != request.ProductId)
            throw new NotFoundException($"Photo '{request.PhotoId}' not found.");

        if (!request.IsAdmin && photo.Product.SellerId != request.SellerId)
            throw new ForbiddenException("You do not own this product.");

        var allPhotos = await db.ProductPhotos
            .Where(p => p.ProductId == request.ProductId)
            .ToListAsync(ct);

        foreach (var p in allPhotos)
            p.IsMain = p.Id == request.PhotoId;

        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync($"product:{request.ProductId}", ct);
    }
}
