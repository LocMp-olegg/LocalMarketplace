using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace LocMp.Order.Infrastructure.Extensions;

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

        public IEnumerable<string> GetUserRoles() =>
            context.User.FindAll(ClaimTypes.Role).Select(c => c.Value);

        public bool IsAdmin() =>
            context.User.IsInRole("Admin");
    }
}
