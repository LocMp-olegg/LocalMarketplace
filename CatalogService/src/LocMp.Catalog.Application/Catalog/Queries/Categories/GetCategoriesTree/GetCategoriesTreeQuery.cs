using LocMp.BuildingBlocks.Application.Common;
using LocMp.Catalog.Application.DTOs;
using MediatR;

namespace LocMp.Catalog.Application.Catalog.Queries.Categories.GetCategoriesTree;

public sealed record GetCategoriesTreeQuery : IRequest<IReadOnlyList<CategoryTreeDto>>, ICacheableQuery
{
    public string CacheKey => "categories:all";
    public TimeSpan Ttl => TimeSpan.FromHours(1);
}
