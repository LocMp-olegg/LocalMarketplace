using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;

namespace LocMp.Catalog.Application.Catalog.Queries.Sellers.GetSeller;

public sealed class GetSellerQueryHandler(CatalogDbContext db)
    : IRequestHandler<GetSellerQuery, SellerDto>
{
    public async Task<SellerDto> Handle(GetSellerQuery request, CancellationToken ct)
    {
        var seller = await db.SellerReadModels.FindAsync([request.SellerId], ct)
                     ?? throw new NotFoundException($"Seller '{request.SellerId}' not found.");

        return new SellerDto(
            seller.Id,
            seller.DisplayName,
            seller.AvatarUrl,
            seller.AverageRating,
            seller.ReviewCount,
            seller.Location?.Y,
            seller.Location?.X,
            seller.LastSyncedAt
        );
    }
}
