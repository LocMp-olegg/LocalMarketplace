using LocMp.Gateway.Api.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();


builder.Configuration.AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", optional: false,
    reloadOnChange: true);

builder.Host.AddSerilogLogging();

builder.Services.AddControllers();
builder.Services.ConfigureOptions(builder.Configuration);
builder.Services.AddCorsConfiguration();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerGen();
builder.Services.AddGateway(builder.Configuration);
builder.Services.AddRedisBlocklist(builder.Configuration);

var app = builder.Build();

app.UseStaticFiles();
app.UseCors();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerForOcelotUI();
}

app.MapControllers();

app.Use(async (context, next) =>
{
    var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    context.Request.Headers.TryAdd("X-User-IP", ip);
    await next();
});

app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                     ?? context.User.FindFirst("sub")?.Value;
        if (userId is not null)
        {
            var cache = context.RequestServices.GetRequiredService<IDistributedCache>();
            var blocked = await cache.GetAsync($"locmp:blocked:{userId}", context.RequestAborted);
            if (blocked is not null)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }
        }
    }
    await next();
});

await app.UseOcelot();

await app.RunAsync();