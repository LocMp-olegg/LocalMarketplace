using LocMp.Catalog.Api.Requests.Shops;
using LocMp.Catalog.Application.Catalog.Commands.Shops.CreateShop;
using LocMp.Catalog.Application.Catalog.Commands.Shops.UpdateShop;
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
    /// <summary>Получить магазин по Id.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ShopDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await sender.Send(new GetShopByIdQuery(id), ct));

    /// <summary>Мои магазины (из JWT).</summary>
    [HttpGet("my")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<IReadOnlyList<ShopDto>>> GetMy(CancellationToken ct)
        => Ok(await sender.Send(new GetShopsBySellerQuery(HttpContext.GetUserId()), ct));

    /// <summary>Магазины продавца по SellerId.</summary>
    [HttpGet("by-seller/{sellerId:guid}")]
    public async Task<ActionResult<IReadOnlyList<ShopDto>>> GetBySeller(Guid sellerId, CancellationToken ct)
        => Ok(await sender.Send(new GetShopsBySellerQuery(sellerId), ct));

    /// <summary>Создать магазин. Требуется роль Seller.</summary>
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

    /// <summary>Обновить магазин.</summary>
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
}
