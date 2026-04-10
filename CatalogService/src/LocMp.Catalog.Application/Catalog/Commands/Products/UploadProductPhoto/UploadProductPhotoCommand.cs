using MediatR;
using Microsoft.AspNetCore.Http;

namespace LocMp.Catalog.Application.Catalog.Commands.Products.UploadProductPhoto;

public sealed record UploadProductPhotoCommand(
    Guid ProductId,
    Guid SellerId,
    IReadOnlyList<IFormFile> Photos,
    bool IsAdmin = false
) : IRequest<IReadOnlyList<Guid>>;
