using LocMp.Catalog.Application.DTOs;
using MediatR;

namespace LocMp.Catalog.Application.Catalog.Queries.Shops.GetShopsBySeller;

public sealed record GetShopsBySellerQuery(Guid SellerId) : IRequest<IReadOnlyList<ShopDto>>;
