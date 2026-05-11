using MediatR;

namespace LocMp.Identity.Application.Identity.Commands.Auth.ResetPassword;

public sealed record ResetPasswordCommand(string Email, string Token, string NewPassword) : IRequest<Unit>;
