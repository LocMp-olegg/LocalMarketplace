using MediatR;

namespace LocMp.Catalog.Application.Catalog.Commands.Shops.SetSellerDelivery;

public sealed record SetSellerDeliveryCommand(
    Guid ShopId,
    Guid RequesterId,
    bool IsAdmin,
    bool Allow) : IRequest;