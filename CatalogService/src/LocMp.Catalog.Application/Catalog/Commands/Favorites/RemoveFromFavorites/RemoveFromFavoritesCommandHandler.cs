using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Catalog.Infrastructure.Persistence;
using LocMp.Contracts.Catalog;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Catalog.Application.Catalog.Commands.Favorites.RemoveFromFavorites;

public sealed class RemoveFromFavoritesCommandHandler(CatalogDbContext db, IEventBus eventBus)
    : IRequestHandler<RemoveFromFavoritesCommand>
{
    public async Task Handle(RemoveFromFavoritesCommand request, CancellationToken ct)
    {
        var favorite = await db.Favorites
            .Include(f => f.Product)
            .FirstOrDefaultAsync(f => f.UserId == request.UserId && f.ProductId == request.ProductId, ct)
            ?? throw new NotFoundException("Product not found in favorites.");

        db.Favorites.Remove(favorite);
        await db.SaveChangesAsync(ct);

        await eventBus.PublishAsync(
            new FavoriteChangedEvent(favorite.ProductId, favorite.Product.SellerId, request.UserId, false, DateTimeOffset.UtcNow), ct);
    }
}
