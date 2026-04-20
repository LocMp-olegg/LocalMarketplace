using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Catalog.Domain.Entities;
using LocMp.Catalog.Infrastructure.Persistence;
using LocMp.Contracts.Catalog;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Catalog.Application.Catalog.Commands.Favorites.AddToFavorites;

public sealed class AddToFavoritesCommandHandler(CatalogDbContext db, IEventBus eventBus)
    : IRequestHandler<AddToFavoritesCommand, Guid>
{
    public async Task<Guid> Handle(AddToFavoritesCommand request, CancellationToken ct)
    {
        var product = await db.Products
            .FirstOrDefaultAsync(p => p.Id == request.ProductId && !p.IsDeleted && p.IsActive, ct)
            ?? throw new NotFoundException($"Product '{request.ProductId}' not found.");

        var alreadyExists = await db.Favorites
            .AnyAsync(f => f.UserId == request.UserId && f.ProductId == request.ProductId, ct);
        if (alreadyExists)
            throw new ConflictException("Product is already in favorites.");

        var favorite = new Favorite(Guid.NewGuid())
        {
            UserId = request.UserId,
            ProductId = request.ProductId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.Favorites.Add(favorite);
        await db.SaveChangesAsync(ct);

        await eventBus.PublishAsync(
            new FavoriteChangedEvent(product.Id, product.SellerId, request.UserId, true, DateTimeOffset.UtcNow), ct);

        return favorite.Id;
    }
}
