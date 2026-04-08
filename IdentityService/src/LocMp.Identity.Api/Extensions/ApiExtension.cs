using System.Text;
using System.Text.Json.Serialization;
using Duende.IdentityServer;
using LocMp.Identity.Api.Handlers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

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