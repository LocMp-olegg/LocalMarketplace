using MediatR;

namespace LocMp.Catalog.Application.Catalog.Commands.Favorites.AddToFavorites;

public sealed record AddToFavoritesCommand(Guid UserId, Guid ProductId) : IRequest<Guid>;
