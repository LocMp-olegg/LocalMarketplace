using MediatR;

namespace LocMp.Catalog.Application.Catalog.Commands.Products.DeleteProductPhoto;

public sealed record DeleteProductPhotoCommand(Guid PhotoId, Guid RequesterId, bool IsAdmin = false) : IRequest;
