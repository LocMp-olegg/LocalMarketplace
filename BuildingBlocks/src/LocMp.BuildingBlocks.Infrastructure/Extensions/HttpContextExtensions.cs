using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace LocMp.BuildingBlocks.Infrastructure.Extensions;

public static class HttpContextExtensions
{
    public static string? GetUserIdString(this ClaimsPrincipal user)
        => user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");

    extension(HttpContext context)
    {
        public Guid GetUserId()
        {
            var userId = context.User.GetUserIdString();
            return userId is null
                ? throw new UnauthorizedAccessException("User ID claim is missing.")
                : Guid.Parse(userId);
        }

        public string GetUserEmail() =>
            context.User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

        public string GetUserName() =>
            context.User.FindFirstValue("username") ?? string.Empty;

        public IEnumerable<string> GetUserRoles() =>
            context.User.FindAll(ClaimTypes.Role).Select(c => c.Value);

        public bool IsInRole(string role) =>
            context.User.IsInRole(role);
    }
}
