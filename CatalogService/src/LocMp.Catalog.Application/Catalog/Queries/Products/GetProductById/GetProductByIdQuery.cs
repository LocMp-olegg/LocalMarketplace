using LocMp.BuildingBlocks.Application.Common;
using LocMp.Catalog.Application.DTOs;
using MediatR;

namespace LocMp.Catalog.Application.Catalog.Queries.Products.GetProductById;

public sealed record GetProductByIdQuery(Guid Id, Guid? ViewerId = null) : IRequest<ProductDto>, ICacheableQuery
{
    public string CacheKey => $"product:{Id}";
    public TimeSpan Ttl => TimeSpan.FromMinutes(5);
}
