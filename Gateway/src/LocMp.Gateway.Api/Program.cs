using LocMp.Gateway.Api.Extensions;
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

await app.UseOcelot();

await app.RunAsync();