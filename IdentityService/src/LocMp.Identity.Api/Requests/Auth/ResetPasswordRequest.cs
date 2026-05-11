namespace LocMp.Identity.Api.Requests.Auth;

public sealed record ResetPasswordRequest(string Email, string Token, string NewPassword);
