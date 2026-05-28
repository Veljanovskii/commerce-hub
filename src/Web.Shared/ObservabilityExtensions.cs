using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Web.Shared.Diagnostics;

namespace Web.Shared;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddCommerceHubObservability(this IServiceCollection services, string serviceName)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSource(CommerceHubDiagnostics.ApplicationSource.Name)
                .AddSource(CommerceHubDiagnostics.InfrastructureSource.Name))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddMeter("CommerceHub.Metrics"));

        return services;
    }
}
