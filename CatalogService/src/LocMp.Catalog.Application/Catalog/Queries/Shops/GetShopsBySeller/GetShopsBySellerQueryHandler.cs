using AutoMapper;
using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Catalog.Application.Catalog.Queries.Shops.GetShopsBySeller;

public sealed class GetShopsBySellerQueryHandler(CatalogDbContext db, IMapper mapper)
    : IRequestHandler<GetShopsBySellerQuery, IReadOnlyList<ShopDto>>
{
    public async Task<IReadOnlyList<ShopDto>> Handle(GetShopsBySellerQuery request, CancellationToken ct)
    {
        var shops = await db.Shops
            .Where(s => s.SellerId == request.SellerId)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync(ct);

        return mapper.Map<List<ShopDto>>(shops);
    }
}
