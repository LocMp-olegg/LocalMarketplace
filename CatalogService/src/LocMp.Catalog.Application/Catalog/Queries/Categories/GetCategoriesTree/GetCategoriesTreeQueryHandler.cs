using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Domain.Entities;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Catalog.Application.Catalog.Queries.Categories.GetCategoriesTree;

public sealed class GetCategoriesTreeQueryHandler(CatalogDbContext db)
    : IRequestHandler<GetCategoriesTreeQuery, IReadOnlyList<CategoryTreeDto>>
{
    public async Task<IReadOnlyList<CategoryTreeDto>> Handle(
        GetCategoriesTreeQuery request, CancellationToken ct)
    {
        var allCategories = await db.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(ct);

        return BuildTree(allCategories, null);
    }

    private static IReadOnlyList<CategoryTreeDto> BuildTree(
        IReadOnlyList<Category> all, Guid? parentId)
    {
        return all
            .Where(c => c.ParentCategoryId == parentId)
            .Select(c => new CategoryTreeDto(
                c.Id,
                c.ParentCategoryId,
                c.Name,
                c.Description,
                c.ImageUrl,
                c.SortOrder,
                c.IsActive,
                BuildTree(all, c.Id)
            ))
            .ToList();
    }
}
