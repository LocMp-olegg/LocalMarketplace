using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Catalog.Infrastructure.Events;
using LocMp.Catalog.Infrastructure.Options;
using LocMp.Catalog.Infrastructure.Persistence;
using LocMp.Catalog.Infrastructure.Storage;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;

namespace LocMp.Catalog.Infrastructure.Extensions;

public static class InfrastructureExtension
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("LocalMarketplaceDb")
                               ?? throw new InvalidOperationException(
                                   "Connection string 'LocalMarketplaceDb' not found.");

        services.AddDbContext<CatalogDbContext>(options =>
            options.UseNpgsql(connectionString, x => x.UseNetTopologySuite()));

        var rabbitMqHost = configuration.GetConnectionString("RabbitMq")
                           ?? "amqp://guest:guest@localhost:5672";

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(new Uri(rabbitMqHost));
                cfg.ConfigureEndpoints(ctx);
            });
        });

        services.AddScoped<IEventBus, MassTransitEventBus>();

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

        // Redis
        var redisConnection = configuration.GetConnectionString("Redis")
                              ?? "localhost:6379";
        services.AddStackExchangeRedisCache(opts =>
        {
            opts.Configuration = redisConnection;
            opts.InstanceName = "locmp-catalog:";
        });
    }
}