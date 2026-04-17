using LocMp.Analytics.Api.Extensions;
using LocMp.Analytics.Application.Extensions;
using LocMp.Analytics.Infrastructure.Extensions;
using LocMp.Analytics.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Context;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build())
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    var configuration = builder.Configuration;

    builder.Host.UseSerilog();

    builder.Services.AddInfrastructure(configuration);
    builder.Services.AddApplication();
    builder.Services.AddApi(configuration);
    builder.Services.AddAuth(configuration);

    builder.Services.AddOpenApi();

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    app.UseHttpsRedirection();

    app.Use(async (ctx, next) =>
    {
        using (LogContext.PushProperty("TraceId", ctx.TraceIdentifier))
            await next();
    });

    app.UseSerilogRequestLogging();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseExceptionHandler();
    app.MapControllers();

    Log.Information("AnalyticsService started successfully");

    app.Run();
}
catch (Exception ex) when (ex is not OperationCanceledException && ex.GetType().Name != "HostAbortedException")
{
    Log.Fatal(ex, "AnalyticsService terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}