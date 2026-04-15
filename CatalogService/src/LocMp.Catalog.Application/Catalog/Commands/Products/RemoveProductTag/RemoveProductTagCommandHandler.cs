using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace LocMp.Catalog.Application.Catalog.Commands.Products.RemoveProductTag;

public sealed class RemoveProductTagCommandHandler(CatalogDbContext db, IDistributedCache cache)
    : IRequestHandler<RemoveProductTagCommand>
{
    public async Task Handle(RemoveProductTagCommand request, CancellationToken ct)
    {
        var product = await db.Products.FindAsync([request.ProductId], ct);
        if (product is null || product.IsDeleted)
            throw new NotFoundException($"Product '{request.ProductId}' not found.");

        if (!request.IsAdmin && product.SellerId != request.RequesterId)
            throw new ForbiddenException("You do not own this product.");

        var productTag = await db.ProductTags
            .FirstOrDefaultAsync(pt => pt.ProductId == request.ProductId && pt.TagId == request.TagId, ct)
            ?? throw new NotFoundException($"Tag '{request.TagId}' not found on this product.");

        db.ProductTags.Remove(productTag);
        await db.SaveChangesAsync(ct);

        await cache.RemoveAsync($"product:{request.ProductId}", ct);
        await cache.RemoveAsync("tags:all", ct);
    }
}
