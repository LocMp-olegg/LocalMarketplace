using LocMp.BuildingBlocks.Application.Common;
using LocMp.Catalog.Application.DTOs;
using MediatR;

namespace LocMp.Catalog.Application.Catalog.Queries.Sellers.GetSeller;

public sealed record GetSellerQuery(Guid SellerId) : IRequest<SellerDto>, ICacheableQuery
{
    public string CacheKey => $"seller:{SellerId}";
    public TimeSpan Ttl => TimeSpan.FromMinutes(10);
}
