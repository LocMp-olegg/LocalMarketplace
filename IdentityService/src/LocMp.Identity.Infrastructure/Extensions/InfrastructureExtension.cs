using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.BuildingBlocks.Infrastructure.Events;
using LocMp.BuildingBlocks.Infrastructure.Options;
using LocMp.BuildingBlocks.Infrastructure.Storage;
using LocMp.Identity.Infrastructure.BackgroundServices;
using LocMp.Identity.Infrastructure.Options;
using LocMp.Identity.Infrastructure.Persistence;
using LocMp.Identity.Infrastructure.Services;
using MassTransit;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;

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

        var rabbitMqHost = configuration.GetConnectionString("RabbitMq")
                           ?? "amqp://guest:guest@localhost:5672";

        services.AddMassTransit(x =>
        {
            x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("identity", false));

            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(new Uri(rabbitMqHost));
                cfg.UseMessageRetry(r => { r.Interval(3, TimeSpan.FromSeconds(2)); });
                cfg.ConfigureEndpoints(ctx);
            });
        });

        services.AddScoped<IEventBus, MassTransitEventBus>();
        services.AddScoped<ISessionService, SessionService>();

        var redisConnection = configuration.GetConnectionString("Redis")
                              ?? throw new InvalidOperationException("Connection string 'Redis' not found.");
        services.AddStackExchangeRedisCache(o => o.Configuration = redisConnection);

        // MinIO
        services.Configure<MinioOptions>(configuration.GetSection("Minio"));
        var minioOpts = configuration.GetSection("Minio").Get<MinioOptions>()
                        ?? throw new InvalidOperationException("Minio configuration section is missing.");

        services.AddMinio(client => client
            .WithEndpoint(minioOpts.Endpoint)
            .WithCredentials(minioOpts.AccessKey, minioOpts.SecretKey)
            .WithSSL(minioOpts.UseSSL)
            .Build());

        services.AddScoped<IStorageService, MinioStorageService>();
    }
}