using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Web.Shared.Diagnostics;

public static class CommerceHubDiagnostics
{
    public const string ServiceName = "CommerceHub";

    public static readonly ActivitySource ApplicationSource = new("CommerceHub.Application");
    public static readonly ActivitySource InfrastructureSource = new("CommerceHub.Infrastructure");

    private static readonly Meter Meter = new("CommerceHub.Metrics");

    public static readonly Histogram<long> ResponsePayloadSize =
        Meter.CreateHistogram<long>(
            "commerce_hub.response.bytes",
            unit: "By",
            description: "Response payload size in bytes");

    public static readonly Counter<long> BackendCalls =
        Meter.CreateCounter<long>(
            "commerce_hub.backend.calls",
            unit: "{call}",
            description: "Number of backend service calls (DB queries, HTTP) per request");
}
