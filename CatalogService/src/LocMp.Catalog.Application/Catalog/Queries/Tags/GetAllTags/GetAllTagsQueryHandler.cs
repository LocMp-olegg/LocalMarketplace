using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Catalog.Application.Catalog.Queries.Tags.GetAllTags;

public sealed class GetAllTagsQueryHandler(CatalogDbContext db)
    : IRequestHandler<GetAllTagsQuery, IReadOnlyList<TagDto>>
{
    public async Task<IReadOnlyList<TagDto>> Handle(GetAllTagsQuery request, CancellationToken ct)
    {
        return await db.Tags
            .OrderBy(t => t.Name)
            .Select(t => new TagDto(
                t.Id,
                t.Name,
                t.Slug,
                t.ProductTags.Count(pt => pt.Product.IsActive && !pt.Product.IsDeleted)
            ))
            .ToListAsync(ct);
    }
}
