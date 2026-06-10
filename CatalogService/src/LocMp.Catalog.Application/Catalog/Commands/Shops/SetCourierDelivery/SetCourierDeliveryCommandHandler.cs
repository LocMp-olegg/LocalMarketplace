using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;

namespace LocMp.Catalog.Application.Catalog.Commands.Shops.SetCourierDelivery;

public sealed class SetCourierDeliveryCommandHandler(CatalogDbContext db)
    : IRequestHandler<SetCourierDeliveryCommand>
{
    public async Task Handle(SetCourierDeliveryCommand request, CancellationToken ct)
    {
        var shop = await db.Shops.FindAsync([request.ShopId], ct)
                   ?? throw new NotFoundException($"Shop '{request.ShopId}' not found.");

        if (!request.IsAdmin && shop.SellerId != request.RequesterId)
            throw new ForbiddenException("You can only modify your own shops.");

        shop.AllowCourierDelivery = request.Allow;
        shop.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
    }
}
