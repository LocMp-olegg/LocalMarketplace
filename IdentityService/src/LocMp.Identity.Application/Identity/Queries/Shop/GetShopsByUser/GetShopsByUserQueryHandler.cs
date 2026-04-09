using LocMp.Identity.Application.DTOs.Shop;
using LocMp.Identity.Application.Identity.Commands.Shop.CreateShop;
using LocMp.Identity.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Identity.Application.Identity.Queries.Shop.GetShopsByUser;

public sealed class GetShopsByUserQueryHandler(ApplicationDbContext db)
    : IRequestHandler<GetShopsByUserQuery, IReadOnlyList<ShopProfileDto>>
{
    public async Task<IReadOnlyList<ShopProfileDto>> Handle(
        GetShopsByUserQuery request, CancellationToken ct)
    {
        var shops = await db.ShopProfiles
            .Where(s => s.UserId == request.UserId)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync(ct);

        return shops.Select(CreateShopCommandHandler.ToDto).ToList();
    }
}
