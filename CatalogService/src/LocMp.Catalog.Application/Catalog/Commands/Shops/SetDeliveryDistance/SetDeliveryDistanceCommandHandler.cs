using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;

namespace LocMp.Catalog.Application.Catalog.Commands.Shops.SetDeliveryDistance;

public sealed class SetDeliveryDistanceCommandHandler(CatalogDbContext db)
    : IRequestHandler<SetDeliveryDistanceCommand>
{
    public async Task Handle(SetDeliveryDistanceCommand request, CancellationToken ct)
    {
        var shop = await db.Shops.FindAsync([request.ShopId], ct)
                   ?? throw new NotFoundException($"Shop '{request.ShopId}' not found.");

        if (!request.IsAdmin && shop.SellerId != request.RequesterId)
            throw new ForbiddenException("You can only modify your own shops.");

        shop.MaxCourierDistanceMeters = request.MaxDistanceMeters;
        shop.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
    }
}
