using LocMp.Identity.Api.Requests;
using LocMp.Identity.Api.Requests.UserProfile;
using LocMp.Identity.Application.DTOs.UserProfile;
using LocMp.Identity.Application.Identity.Commands.UserProfile.DeleteUserPhoto;
using LocMp.Identity.Application.Identity.Commands.UserProfile.UpdateUserProfile;
using LocMp.Identity.Application.Identity.Commands.UserProfile.UploadUserPhoto;
using LocMp.Identity.Application.Identity.Queries.UserProfile.GetUserPhoto;
using LocMp.Identity.Application.Identity.Queries.UserProfile.GetUserProfile;
using LocMp.Identity.Infrastructure.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocMp.Identity.Api.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class UserProfileController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<UserProfileDto>> GetProfile(CancellationToken ct)
    {
        var result = await mediator.Send(new GetUserProfileQuery(HttpContext.GetUserId()), ct);
        return Ok(result);
    }

    [HttpPatch]
    public async Task<ActionResult<UserProfileDto>> UpdateProfile([FromBody] UpdateUserProfileRequest request,
        CancellationToken ct)
    {
        var command = new UpdateUserProfileCommand(
            UserId: HttpContext.GetUserId(),
            FirstName: request.FirstName,
            LastName: request.LastName,
            Gender: request.Gender,
            BirthDate: request.BirthDate,
            PhoneNumber: request.PhoneNumber
        );

        var result = await mediator.Send(command, ct);
        return Ok(result);
    }

    // TODO: Вероятно потребуется доработка с кэшированием
    [HttpGet("photo")]
    [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 3600)]
    public async Task<IActionResult> GetPhoto(CancellationToken ct)
    {
        var result = await mediator.Send(new GetUserPhotoQuery(HttpContext.GetUserId()), ct);
        var etag = $"\"{result.UploadedAt.Ticks}\"";
        return File(result.PhotoData, result.MimeType,
            lastModified: result.UploadedAt,
            entityTag: new Microsoft.Net.Http.Headers.EntityTagHeaderValue(etag));
    }

    [HttpPost("photo")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadPhoto(IFormFile photo, CancellationToken ct)
    {
        await mediator.Send(new UploadUserPhotoCommand(HttpContext.GetUserId(), photo), ct);
        return NoContent();
    }

    [HttpDelete("photo")]
    public async Task<IActionResult> DeletePhoto(CancellationToken ct)
    {
        await mediator.Send(new DeleteUserPhotoCommand(HttpContext.GetUserId()), ct);
        return NoContent();
    }
}