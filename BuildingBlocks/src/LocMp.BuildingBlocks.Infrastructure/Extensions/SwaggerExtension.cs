using LocMp.BuildingBlocks.Infrastructure.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LocMp.BuildingBlocks.Infrastructure.Extensions;

public static class SwaggerExtension
{
    public static void AddSwagger(this IServiceCollection services, IConfiguration configuration,
        Action<SwaggerGenOptions>? configure = null)
    {
        services.AddEndpointsApiExplorer();

        var swaggerOptions = configuration.GetSection("SwaggerDocOptions").Get<SwaggerDocOptions>()
                             ?? throw new InvalidOperationException(
                                 "SwaggerDocOptions section is missing in appsettings.json");

        services.AddSwaggerGen(opt =>
        {
            opt.SwaggerDoc(swaggerOptions.Name, new OpenApiInfo
            {
                Version = swaggerOptions.Version,
                Title = swaggerOptions.Title,
                Description = swaggerOptions.Description
            });

            foreach (var server in swaggerOptions.Servers)
            {
                opt.AddServer(new OpenApiServer
                {
                    Url = server.Url,
                    Description = server.Description
                });
            }

            opt.EnableAnnotations();

            configure?.Invoke(opt);
        });
    }

    public static void UseSwaggerUi(this IApplicationBuilder app, IConfiguration configuration)
    {
        var swaggerOptions = configuration.GetSection("SwaggerDocOptions").Get<SwaggerDocOptions>()
                             ?? throw new InvalidOperationException(
                                 "SwaggerDocOptions section is missing in appsettings.json");

        app.UseSwagger();
        app.UseSwaggerUI(opt =>
        {
            opt.SwaggerEndpoint($"/swagger/{swaggerOptions.Name}/swagger.json",
                $"{swaggerOptions.Title} {swaggerOptions.Version}");
        });
    }
}
