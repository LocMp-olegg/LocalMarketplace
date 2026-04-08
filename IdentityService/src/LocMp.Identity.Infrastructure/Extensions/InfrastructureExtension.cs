using LocMp.Identity.Infrastructure.BackgroundServices;
using LocMp.Identity.Infrastructure.Options;
using LocMp.Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LocMp.Identity.Infrastructure.Extensions;

public static class InfrastructureExtension
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("LocalMarketplaceDb")
                               ?? throw new InvalidOperationException(
                                   "Connection string 'LocalMarketplaceDb' not found.");

        var keysDirectory = Path.Combine(Directory.GetCurrentDirectory(), "keys");
        if (!Directory.Exists(keysDirectory))
            Directory.CreateDirectory(keysDirectory);

        services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(keysDirectory))
            .SetApplicationName("LocMp-Identity");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(
                connectionString,
                x => x.UseNetTopologySuite());
        });

        services.Configure<IdentityServerSettings>(configuration.GetSection("IdentityServer"));

        services.AddHostedService<UserUnblockingBackgroundService>();
    }
}