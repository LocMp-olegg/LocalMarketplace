using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Domain.Entities;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace LocMp.Catalog.Application.Catalog.Commands.Products.AddProductTag;

public sealed class AddProductTagCommandHandler(CatalogDbContext db, IDistributedCache cache)
    : IRequestHandler<AddProductTagCommand, TagDto>
{
    public async Task<TagDto> Handle(AddProductTagCommand request, CancellationToken ct)
    {
        var product = await db.Products.FindAsync([request.ProductId], ct)
                      ?? throw new NotFoundException($"Product '{request.ProductId}' not found.");

        if (!request.IsAdmin && product.SellerId != request.RequesterId)
            throw new ForbiddenException("You do not own this product.");

        var normalizedName = request.TagName.Trim().ToLowerInvariant();
        var slug = normalizedName.Replace(" ", "-");

        var tag = await db.Tags.FirstOrDefaultAsync(t => t.Slug == slug, ct);
        if (tag is null)
        {
            tag = new Tag(Guid.NewGuid()) { Name = normalizedName, Slug = slug };
            db.Tags.Add(tag);
        }

        var exists = await db.ProductTags
            .AnyAsync(pt => pt.ProductId == request.ProductId && pt.TagId == tag.Id, ct);

        if (!exists)
            db.ProductTags.Add(new ProductTag { ProductId = product.Id, TagId = tag.Id });

        await db.SaveChangesAsync(ct);

        await cache.RemoveAsync($"product:{request.ProductId}", ct);
        await cache.RemoveAsync("tags:all", ct);

        var productCount = await db.ProductTags.CountAsync(pt => pt.TagId == tag.Id, ct);
        return new TagDto(tag.Id, tag.Name, tag.Slug, productCount);
    }
}