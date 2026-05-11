using System.Net;
using LocMp.Identity.Domain.Entities;
using LocMp.Identity.Infrastructure.Options;
using LocMp.Identity.Infrastructure.Services;
using LocMp.Identity.Infrastructure.Templates;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace LocMp.Identity.Application.Identity.Commands.Auth.ForgotPassword;

public sealed class ForgotPasswordCommandHandler(
    UserManager<ApplicationUser> userManager,
    IEmailService emailService,
    IOptions<PasswordResetOptions> options
) : IRequestHandler<ForgotPasswordCommand, Unit>
{
    public async Task<Unit> Handle(ForgotPasswordCommand request, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user is null || !user.Active)
            return Unit.Value;

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = BuildResetLink(request.Email, token, options.Value.UiBaseUrl);
        var name = string.IsNullOrWhiteSpace(user.FirstName) ? user.UserName! : user.FirstName;

        await emailService.SendAsync(
            user.Email!,
            "Сброс пароля — Районный",
            EmailTemplates.PasswordReset(name, resetLink),
            ct);

        return Unit.Value;
    }

    private static string BuildResetLink(string email, string token, string uiBaseUrl)
    {
        var encodedToken = WebUtility.UrlEncode(token);
        var encodedEmail = WebUtility.UrlEncode(email);
        return $"{uiBaseUrl.TrimEnd('/')}/reset-password?token={encodedToken}&email={encodedEmail}";
    }
}
