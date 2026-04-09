using LocMp.Identity.Application.DTOs.Shop;
using MediatR;

namespace LocMp.Identity.Application.Identity.Queries.Shop.GetShopsByUser;

public sealed record GetShopsByUserQuery(Guid UserId) : IRequest<IReadOnlyList<ShopProfileDto>>;
