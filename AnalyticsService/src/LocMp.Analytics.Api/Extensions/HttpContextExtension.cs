using System.Security.Claims;

namespace LocMp.Analytics.Api.Extensions;

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

        public bool IsInRole(string role) =>
            context.User.IsInRole(role);
    }
}
