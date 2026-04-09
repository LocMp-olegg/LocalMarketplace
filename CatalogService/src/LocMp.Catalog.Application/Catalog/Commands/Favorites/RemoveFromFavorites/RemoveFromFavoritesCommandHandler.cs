using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Catalog.Application.Catalog.Commands.Favorites.RemoveFromFavorites;

public sealed class RemoveFromFavoritesCommandHandler(CatalogDbContext db)
    : IRequestHandler<RemoveFromFavoritesCommand>
{
    public async Task Handle(RemoveFromFavoritesCommand request, CancellationToken ct)
    {
        var favorite = await db.Favorites
            .FirstOrDefaultAsync(f => f.UserId == request.UserId && f.ProductId == request.ProductId, ct)
            ?? throw new NotFoundException("Product not found in favorites.");

        db.Favorites.Remove(favorite);
        await db.SaveChangesAsync(ct);
    }
}
