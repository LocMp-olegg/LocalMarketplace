using LocMp.BuildingBlocks.Infrastructure.Extensions;
using LocMp.Order.Api.Extensions;
using LocMp.Order.Application.Extensions;
using LocMp.Order.Infrastructure.Extensions;
using LocMp.Order.Infrastructure.Persistence;
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
    builder.Services.AddSwagger(configuration);

    builder.Services.AddOpenApi();

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    app.UseSwaggerUi(configuration);

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

    Log.Information("OrderService started successfully");

    app.Run();
}
catch (Exception ex) when (ex is not OperationCanceledException && ex.GetType().Name != "HostAbortedException")
{
    Log.Fatal(ex, "OrderService terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
