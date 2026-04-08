using System.Text.Json.Serialization;
using LocMp.BuildingBlocks.Infrastructure.Middleware;

namespace LocMp.Identity.Api.Extensions;

public static class ApiExtension
{
    public static void AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
    }
}