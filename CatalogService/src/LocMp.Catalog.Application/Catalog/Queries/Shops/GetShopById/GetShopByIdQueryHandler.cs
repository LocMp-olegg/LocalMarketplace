using AutoMapper;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Catalog.Application.Catalog.Queries.Shops.GetShopById;

public sealed class GetShopByIdQueryHandler(CatalogDbContext db, IMapper mapper)
    : IRequestHandler<GetShopByIdQuery, ShopDto>
{
    public async Task<ShopDto> Handle(GetShopByIdQuery request, CancellationToken ct)
    {
        var shop = await db.Shops
                       .Include(s => s.Photos)
                       .FirstOrDefaultAsync(s => s.Id == request.ShopId, ct)
                   ?? throw new NotFoundException($"Shop '{request.ShopId}' not found.");

        return mapper.Map<ShopDto>(shop);
    }
}