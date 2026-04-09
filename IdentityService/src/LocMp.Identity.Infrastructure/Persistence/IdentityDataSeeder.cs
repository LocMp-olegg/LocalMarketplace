using LocMp.Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LocMp.Identity.Infrastructure.Persistence;

public static class IdentityDataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("IdentitySeeder");

        var roles = new[] { "Admin", "User", "Seller", "Courier" };

        foreach (var roleName in roles)
        {
            if (await roleManager.RoleExistsAsync(roleName)) continue;
            var role = new ApplicationRole
            {
                Name = roleName,
                NormalizedName = roleName.ToUpper(),
                Active = true
            };

            await roleManager.CreateAsync(role);
            logger.LogInformation("Role {Role} created", roleName);
        }

        const string adminEmail = "admin@local.test";

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = "admin",
                Email = adminEmail,
                NormalizedUserName = "ADMIN",
                NormalizedEmail = adminEmail.ToUpper(),
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true,
                Active = true,
                RegisteredAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(admin, "Admin123!");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
                logger.LogInformation("Admin user created");
            }
            else
            {
                logger.LogError("Admin creation failed: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        
        const string userEmail = "user@local.test";

        var user = await userManager.FindByEmailAsync(userEmail);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = "user",
                Email = userEmail,
                NormalizedUserName = "USER",
                NormalizedEmail = userEmail.ToUpper(),
                FirstName = "System",
                LastName = "User",
                EmailConfirmed = true,
                Active = true,
                RegisteredAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, "User123!");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "User");
                logger.LogInformation("User user created");
            }
            else
            {
                logger.LogError("User creation failed: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}