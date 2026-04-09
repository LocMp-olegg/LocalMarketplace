using LocMp.Identity.Api.Requests.Shop;
using LocMp.Identity.Application.DTOs.Shop;
using LocMp.Identity.Application.Identity.Commands.Shop.CreateShop;
using LocMp.Identity.Application.Identity.Queries.Shop.GetShopsByUser;
using LocMp.Identity.Infrastructure.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocMp.Identity.Api.Controllers;

[ApiController]
[Route("api/shops")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class ShopsController(ISender sender) : ControllerBase
{
    /// <summary>Создать магазин для текущего пользователя.</summary>
    [HttpPost]
    public async Task<ActionResult<ShopProfileDto>> Create(
        [FromBody] CreateShopRequest request, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        var command = new CreateShopCommand(
            userId,
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
        return CreatedAtAction(nameof(GetMy), null, result);
    }

    /// <summary>Мои магазины.</summary>
    [HttpGet("my")]
    public async Task<ActionResult<IReadOnlyList<ShopProfileDto>>> GetMy(CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        return Ok(await sender.Send(new GetShopsByUserQuery(userId), ct));
    }

    /// <summary>Магазины пользователя по UserId — только Admin.</summary>
    [HttpGet("user/{userId:guid}")]
    [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<IReadOnlyList<ShopProfileDto>>> GetByUser(Guid userId, CancellationToken ct)
        => Ok(await sender.Send(new GetShopsByUserQuery(userId), ct));
}