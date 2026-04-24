using LocMp.Gateway.Api.Options;
using Microsoft.Extensions.Options;

namespace LocMp.Gateway.Api.Extensions;

public static class CorsExtensions
{
    public static void AddCorsConfiguration(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .WithOrigins(services.BuildServiceProvider().GetRequiredService<IOptions<CorsPoliticsOptions>>()
                        .Value.GetOrigins());
            });
        });
    }
}