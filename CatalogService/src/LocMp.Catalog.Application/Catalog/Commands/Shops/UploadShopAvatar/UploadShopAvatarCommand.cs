using LocMp.Catalog.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LocMp.Catalog.Application.Catalog.Commands.Shops.UploadShopAvatar;

public sealed record UploadShopAvatarCommand(
    Guid ShopId,
    Guid RequesterId,
    bool IsAdmin,
    IFormFile Image
) : IRequest<ShopDto>;
