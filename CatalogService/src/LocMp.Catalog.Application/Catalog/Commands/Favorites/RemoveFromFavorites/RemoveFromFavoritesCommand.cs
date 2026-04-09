using MediatR;

namespace LocMp.Catalog.Application.Catalog.Commands.Favorites.RemoveFromFavorites;

public sealed record RemoveFromFavoritesCommand(Guid UserId, Guid ProductId) : IRequest;
