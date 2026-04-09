using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace LocMp.Catalog.Application.Catalog.Commands.Categories.DeleteCategory;

public sealed class DeleteCategoryCommandHandler(CatalogDbContext db, IDistributedCache cache)
    : IRequestHandler<DeleteCategoryCommand>
{
    public async Task Handle(DeleteCategoryCommand request, CancellationToken ct)
    {
        var category = await db.Categories.FindAsync([request.Id], ct)
                       ?? throw new NotFoundException($"Category '{request.Id}' not found.");

        var hasChildren = await db.Categories.AnyAsync(c => c.ParentCategoryId == request.Id, ct);
        if (hasChildren)
            throw new ConflictException("Cannot delete category that has subcategories.");

        var hasProducts = await db.Products.AnyAsync(p => p.CategoryId == request.Id && !p.IsDeleted, ct);
        if (hasProducts)
            throw new ConflictException("Cannot delete category that has active products.");

        db.Categories.Remove(category);
        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync("categories:all", ct);
        await cache.RemoveAsync($"category:{request.Id}", ct);
    }
}