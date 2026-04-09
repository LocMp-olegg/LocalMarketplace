using MediatR;
using Microsoft.AspNetCore.Http;

namespace LocMp.Catalog.Application.Catalog.Commands.Categories.UploadCategoryImage;

public sealed record UploadCategoryImageCommand(
    Guid CategoryId,
    IFormFile Image
) : IRequest<string>;
