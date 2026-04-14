using System.Security.Claims;

namespace LocMp.Catalog.Api.Extensions;

public static class HttpContextExtensions
{
    extension(HttpContext context)
    {
        public Guid GetUserId()
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? context.User.FindFirstValue("sub");

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
    }
}