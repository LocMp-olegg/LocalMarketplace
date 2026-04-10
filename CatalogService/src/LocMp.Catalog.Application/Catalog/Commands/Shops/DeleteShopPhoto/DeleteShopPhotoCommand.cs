using MediatR;

namespace LocMp.Catalog.Application.Catalog.Commands.Shops.DeleteShopPhoto;

public sealed record DeleteShopPhotoCommand(
    Guid PhotoId,
    Guid RequesterId,
    bool IsAdmin
) : IRequest;
