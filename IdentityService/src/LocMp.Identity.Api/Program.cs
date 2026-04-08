using LocMp.Identity.Api.Extensions;
using LocMp.Identity.Application.Extensions;
using LocMp.Identity.Infrastructure.Extensions;
using LocMp.Identity.Infrastructure.Persistence;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build()).WriteTo.Console()
    .CreateLogger();
try
{
    var builder = WebApplication.CreateBuilder(args);
    var configuration = builder.Configuration;

    builder.Host.UseSerilog();

    builder.Services.AddInfrastructure(configuration);
    builder.Services.AddIdentityConfiguration();
    builder.Services.AddIdentityServerConfiguration(configuration);
    builder.Services.AddApplication();
    builder.Services.AddApi(configuration);
    builder.Services.AddAuth(configuration);
    builder.Services.AddSwagger(configuration);

    builder.Services.AddOpenApi();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwaggerUi(configuration);
        await IdentityDataSeeder.SeedAsync(app.Services);
    }

    app.UseHttpsRedirection();

    app.UseIdentityServer();

    app.UseExceptionHandler();
    app.MapControllers();

    Log.Information("Application started successfully");

    app.Run();
}
catch (Exception ex) when (ex is not OperationCanceledException && ex.GetType().Name != "HostAbortedException")
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}