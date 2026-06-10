using LocMp.BuildingBlocks.Infrastructure.Extensions;
using LocMp.Identity.Api.Requests.Courier;
using LocMp.Identity.Application.DTOs.Courier;
using LocMp.Identity.Application.Identity.Commands.Courier.BecomeACourier;
using LocMp.Identity.Application.Identity.Commands.Courier.ResignCourier;
using LocMp.Identity.Application.Identity.Commands.Courier.UpdateCourierProfile;
using LocMp.Identity.Application.Identity.Queries.Courier.GetCourierProfile;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocMp.Identity.Api.Controllers;

[ApiController]
[Route("api/profile/courier")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class CourierProfileController(ISender sender) : ControllerBase
{
    [HttpPost("become")]
    public async Task<ActionResult<CourierProfileDto>> Become(CancellationToken ct)
    {
        var result = await sender.Send(new BecomeACourierCommand(HttpContext.GetUserId()), ct);
        return Ok(result);
    }

    [HttpGet]
    [Authorize(Roles = "Courier")]
    public async Task<ActionResult<CourierProfileDto>> GetProfile(CancellationToken ct)
    {
        var result = await sender.Send(new GetCourierProfileQuery(HttpContext.GetUserId()), ct);
        return Ok(result);
    }

    [HttpDelete]
    [Authorize(Roles = "Courier")]
    public async Task<IActionResult> Resign(CancellationToken ct)
    {
        await sender.Send(new ResignCourierCommand(HttpContext.GetUserId()), ct);
        return NoContent();
    }

    [HttpPut]
    [Authorize(Roles = "Courier")]
    public async Task<ActionResult<CourierProfileDto>> Update(
        [FromBody] UpdateCourierProfileRequest request, CancellationToken ct)
    {
        var command = new UpdateCourierProfileCommand(
            UserId: HttpContext.GetUserId(),
            IsActive: request.IsActive,
            ServiceRadiusMeters: request.ServiceRadiusMeters,
            Latitude: request.Latitude,
            Longitude: request.Longitude);

        var result = await sender.Send(command, ct);
        return Ok(result);
    }
}