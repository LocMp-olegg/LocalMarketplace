using MediatR;

namespace LocMp.Identity.Application.Identity.Commands.Auth.ForgotPassword;

public sealed record ForgotPasswordCommand(string Email) : IRequest<Unit>;
