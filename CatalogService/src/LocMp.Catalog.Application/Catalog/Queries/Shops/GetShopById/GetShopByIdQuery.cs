using LocMp.Catalog.Application.DTOs;
using MediatR;

namespace LocMp.Catalog.Application.Catalog.Queries.Shops.GetShopById;

public sealed record GetShopByIdQuery(Guid ShopId) : IRequest<ShopDto>;
