using Infrastructure;
using Web.MigrationService;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(MigrationWorker.ActivitySourceName));

builder.Services.AddHostedService<MigrationWorker>();

WebApplication app = builder.Build();

app.MapDefaultEndpoints();

await app.RunAsync();
