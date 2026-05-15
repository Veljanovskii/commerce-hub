using System.Reflection;
using HealthChecks.UI.Client;
using Infrastructure.Seeding;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Web.RestApi.Extensions;
using Web.Api;
using Application;
using Infrastructure;
using Web.Shared;
using Web.Shared.Interceptors;
using Web.Shared.Middleware;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services
    .AddApplication()
    .AddPresentation()
    .AddInfrastructure(builder.Configuration)
    .AddCommerceHubObservability("CommerceHub.RestApi");

builder.Services.AddSingleton<IInterceptor, BackendCallCountingInterceptor>();

builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

WebApplication app = builder.Build();

app.MapDefaultEndpoints();

app.MapEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerWithUi();

    app.ApplyMigrations();
    DataSeeder.SeedData(app.Services);
}

app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseMiddleware<PayloadSizeMiddleware>();

app.UseRequestContextLogging();

app.UseExceptionHandler();

await app.RunAsync();

// REMARK: Required for functional and integration tests to work.
namespace Web.Api
{
    public partial class Program;
}
