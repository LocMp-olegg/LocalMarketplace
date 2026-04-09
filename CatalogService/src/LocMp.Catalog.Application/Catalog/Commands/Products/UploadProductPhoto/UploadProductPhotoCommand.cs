using MediatR;
using Microsoft.AspNetCore.Http;

namespace LocMp.Catalog.Application.Catalog.Commands.Products.UploadProductPhoto;

public sealed record UploadProductPhotoCommand(
    Guid ProductId,
    Guid SellerId,
    IFormFile Photo,
    bool IsMain = false,
    int SortOrder = 0,
    bool IsAdmin = false
) : IRequest<Guid>;
