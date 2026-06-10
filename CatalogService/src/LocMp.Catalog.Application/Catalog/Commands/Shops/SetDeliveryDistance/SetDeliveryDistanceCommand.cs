using MediatR;

namespace LocMp.Catalog.Application.Catalog.Commands.Shops.SetDeliveryDistance;

public sealed record SetDeliveryDistanceCommand(
    Guid ShopId,
    Guid RequesterId,
    bool IsAdmin,
    int? MaxDistanceMeters) : IRequest;
