using LocMp.Identity.Api.Requests.Auth;
using LocMp.Identity.Application.Identity.Commands.Auth.ForgotPassword;
using LocMp.Identity.Application.Identity.Commands.Auth.ResetPassword;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocMp.Identity.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public sealed class AuthController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Запрашивает письмо для сброса пароля.
    /// Всегда возвращает 200, независимо от того, существует ли email.
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken ct)
    {
        await sender.Send(new ForgotPasswordCommand(request.Email), ct);
        return Ok();
    }

    /// <summary>
    /// Устанавливает новый пароль по токену из письма.
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken ct)
    {
        await sender.Send(new ResetPasswordCommand(request.Email, request.Token, request.NewPassword), ct);
        return NoContent();
    }
}
