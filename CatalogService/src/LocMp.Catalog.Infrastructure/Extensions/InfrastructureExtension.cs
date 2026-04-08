using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Catalog.Infrastructure.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LocMp.Catalog.Infrastructure.Persistence;

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
    }
}