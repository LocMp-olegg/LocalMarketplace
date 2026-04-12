using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Order.Infrastructure.BackgroundServices;
using LocMp.Order.Infrastructure.Clients;
using LocMp.Order.Infrastructure.Events;
using LocMp.Order.Infrastructure.Images;
using LocMp.Order.Infrastructure.Options;
using LocMp.Order.Infrastructure.Persistence;
using LocMp.Order.Infrastructure.Storage;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;

namespace LocMp.Order.Infrastructure.Extensions;

public static class InfrastructureExtension
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("LocalMarketplaceDb")
                               ?? throw new InvalidOperationException(
                                   "Connection string 'LocalMarketplaceDb' not found.");

        services.AddDbContext<OrderDbContext>(options =>
            options.UseNpgsql(connectionString, x => x.UseNetTopologySuite()));

        var rabbitMqHost = configuration.GetConnectionString("RabbitMq")
                           ?? "amqp://guest:guest@localhost:5672";

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            x.AddConsumers(typeof(InfrastructureExtension).Assembly);

            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(new Uri(rabbitMqHost));

                cfg.UseMessageRetry(r => { r.Interval(3, TimeSpan.FromSeconds(2)); });

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
        services.AddSingleton<IImageProcessor, ImageSharpProcessor>();

        services.AddHttpClient<CatalogServiceClient>(c =>
            c.BaseAddress = new Uri(configuration["Services:Catalog"]
                ?? throw new InvalidOperationException("Services:Catalog not configured.")));

        var redisConnection = configuration.GetConnectionString("Redis")
                              ?? "localhost:6379";
        services.AddStackExchangeRedisCache(opts =>
        {
            opts.Configuration = redisConnection;
            opts.InstanceName = "locmp-orders:";
        });

        services.Configure<DisputeOptions>(configuration.GetSection("Dispute"));

        services.AddHostedService<CartCleanupBackgroundService>();
        services.AddHostedService<DisputeAutoResolveBackgroundService>();
    }
}