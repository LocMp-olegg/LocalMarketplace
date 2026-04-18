using LocMp.BuildingBlocks.Application.Common;
using LocMp.Catalog.Application.Catalog.Commands.Favorites.AddToFavorites;
using LocMp.Catalog.Application.Catalog.Commands.Favorites.RemoveFromFavorites;
using LocMp.Catalog.Application.Catalog.Queries.Favorites.GetFavoritesByUser;
using LocMp.Catalog.Application.DTOs;
using LocMp.BuildingBlocks.Infrastructure.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocMp.Catalog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class FavoritesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<FavoriteDto>>> GetMyFavorites(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await sender.Send(new GetFavoritesByUserQuery(HttpContext.GetUserId(), page, pageSize), ct));

    [HttpPost("{productId:guid}")]
    public async Task<ActionResult<Guid>> Add(Guid productId, CancellationToken ct)
    {
        var id = await sender.Send(new AddToFavoritesCommand(HttpContext.GetUserId(), productId), ct);
        return CreatedAtAction(nameof(GetMyFavorites), id);
    }

    [HttpDelete("{productId:guid}")]
    public async Task<ActionResult> Remove(Guid productId, CancellationToken ct)
    {
        await sender.Send(new RemoveFromFavoritesCommand(HttpContext.GetUserId(), productId), ct);
        return NoContent();
    }
}
