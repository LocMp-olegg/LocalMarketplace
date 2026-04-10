using LocMp.Catalog.Api.Requests.Shops;
using LocMp.Catalog.Application.Catalog.Commands.Shops.CreateShop;
using LocMp.Catalog.Application.Catalog.Commands.Shops.DeleteShopPhoto;
using LocMp.Catalog.Application.Catalog.Commands.Shops.UpdateShop;
using LocMp.Catalog.Application.Catalog.Commands.Shops.UploadShopAvatar;
using LocMp.Catalog.Application.Catalog.Commands.Shops.UploadShopPhoto;
using LocMp.Catalog.Application.Catalog.Queries.Shops.GetShopById;
using LocMp.Catalog.Application.Catalog.Queries.Shops.GetShopsBySeller;
using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Infrastructure.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocMp.Catalog.Api.Controllers;

[ApiController]
[Route("api/shops")]
public sealed class ShopsController(ISender sender) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ShopDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await sender.Send(new GetShopByIdQuery(id), ct));

    [HttpGet("my")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<IReadOnlyList<ShopDto>>> GetMy(CancellationToken ct)
        => Ok(await sender.Send(new GetShopsBySellerQuery(HttpContext.GetUserId()), ct));

    [HttpGet("by-seller/{sellerId:guid}")]
    public async Task<ActionResult<IReadOnlyList<ShopDto>>> GetBySeller(Guid sellerId, CancellationToken ct)
        => Ok(await sender.Send(new GetShopsBySellerQuery(sellerId), ct));

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Seller,Admin")]
    public async Task<ActionResult<ShopDto>> Create([FromBody] CreateShopRequest request, CancellationToken ct)
    {
        var command = new CreateShopCommand(
            HttpContext.GetUserId(),
            request.BusinessName,
            request.PhoneNumber,
            request.Email,
            request.Description,
            request.Inn,
            request.BusinessType,
            request.WorkingHours,
            request.ServiceRadiusMeters,
            request.Latitude,
            request.Longitude);

        var result = await sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Seller,Admin")]
    public async Task<ActionResult<ShopDto>> Update(Guid id, [FromBody] UpdateShopRequest request, CancellationToken ct)
    {
        var command = new UpdateShopCommand(
            id,
            HttpContext.GetUserId(),
            HttpContext.User.IsInRole("Admin"),
            request.BusinessName,
            request.PhoneNumber,
            request.Email,
            request.Description,
            request.Inn,
            request.BusinessType,
            request.WorkingHours,
            request.ServiceRadiusMeters,
            request.Latitude,
            request.Longitude,
            request.IsActive);

        return Ok(await sender.Send(command, ct));
    }

    [HttpPost("{id:guid}/avatar")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Seller,Admin")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ShopDto>> UploadAvatar(Guid id, IFormFile image, CancellationToken ct)
        => Ok(await sender.Send(new UploadShopAvatarCommand(
            id, HttpContext.GetUserId(), HttpContext.User.IsInRole("Admin"), image), ct));

    [HttpPost("{id:guid}/photos")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Seller,Admin")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ShopPhotoDto>> UploadPhoto(
        Guid id,
        IFormFile image,
        [FromQuery] int sortOrder = 0,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new UploadShopPhotoCommand(
            id, HttpContext.GetUserId(), HttpContext.User.IsInRole("Admin"), image, sortOrder), ct);
        return CreatedAtAction(nameof(GetById), new { id }, result);
    }

    [HttpDelete("{shopId:guid}/photos/{photoId:guid}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Seller,Admin")]
    public async Task<ActionResult> DeletePhoto(Guid shopId, Guid photoId, CancellationToken ct)
    {
        await sender.Send(new DeleteShopPhotoCommand(
            photoId, HttpContext.GetUserId(), HttpContext.User.IsInRole("Admin")), ct);
        return NoContent();
    }
}
