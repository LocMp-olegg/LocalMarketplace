using LocMp.BuildingBlocks.Application.Common;
using LocMp.Catalog.Application.DTOs;
using MediatR;

namespace LocMp.Catalog.Application.Catalog.Queries.Categories.GetCategoryById;

public sealed record GetCategoryByIdQuery(Guid Id) : IRequest<CategoryDto>, ICacheableQuery
{
    public string CacheKey => $"category:{Id}";
    public TimeSpan Ttl => TimeSpan.FromHours(1);
}
