using MediatR;

namespace LocMp.Catalog.Application.Catalog.Commands.Shops.SetCourierDelivery;

public sealed record SetCourierDeliveryCommand(
    Guid ShopId,
    Guid RequesterId,
    bool IsAdmin,
    bool Allow,
    int? MaxDistanceMeters) : IRequest;
