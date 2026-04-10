using LocMp.Catalog.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LocMp.Catalog.Application.Catalog.Commands.Shops.UploadShopPhoto;

public sealed record UploadShopPhotoCommand(
    Guid ShopId,
    Guid RequesterId,
    bool IsAdmin,
    IFormFile Image,
    int SortOrder = 0
) : IRequest<ShopPhotoDto>;
