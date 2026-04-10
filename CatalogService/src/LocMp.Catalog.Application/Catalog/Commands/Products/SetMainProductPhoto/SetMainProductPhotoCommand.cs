using MediatR;

namespace LocMp.Catalog.Application.Catalog.Commands.Products.SetMainProductPhoto;

public sealed record SetMainProductPhotoCommand(
    Guid PhotoId,
    Guid ProductId,
    Guid SellerId,
    bool IsAdmin
) : IRequest;
