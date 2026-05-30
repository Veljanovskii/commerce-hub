using System.Text.Json;
using Benchmarks;
using Benchmarks.Scenarios;
using NBomber.CSharp;

Config.LoadFromEnv();
await Config.DiscoverIdsAsync();

if (string.IsNullOrEmpty(Config.ProductId))
{
    Console.WriteLine("[Benchmark] Cannot run without a ProductId. Exiting.");
    return;
}

const int readRate = 20;
const int writeRate = 5;
const int rampDuration = 20;
const int sustainDuration = 40;

NBomber.Contracts.LoadSimulation[] ReadLoad() =>
[
    Simulation.RampingInject(rate: readRate, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(rampDuration)),
    Simulation.Inject(rate: readRate, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(sustainDuration))
];

NBomber.Contracts.LoadSimulation[] WriteLoad() =>
[
    Simulation.RampingInject(rate: writeRate, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(rampDuration)),
    Simulation.Inject(rate: writeRate, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(sustainDuration))
];

HttpClient CreateClient() => new(new SocketsHttpHandler
{
    MaxConnectionsPerServer = 100,
    PooledConnectionLifetime = TimeSpan.FromMinutes(2)
});

async Task WaitForReady()
{
    await Task.Delay(TimeSpan.FromSeconds(10));
    using HttpClient probe = new();
    try
    {
        await probe.GetAsync($"{Config.RestBaseUrl}/products/{Config.ProductId}");
        await probe.PostAsync($"{Config.GraphQLBaseUrl}/graphql",
            new StringContent("""{"query":"{ __typename }"}""", System.Text.Encoding.UTF8, "application/json"));
    }
    catch { }
}

// ─── Scenario 1: Simple GET ─────────────────────────────────────────────────
Console.WriteLine("\n[Benchmark] ═══ Scenario 1: Simple GET ═══");
using (HttpClient client = CreateClient())
{
    NBomberRunner
        .RegisterScenarios(
            SimpleGetScenario.RestScenario(client).WithLoadSimulations(ReadLoad()),
            SimpleGetScenario.GraphQLScenario(client).WithLoadSimulations(ReadLoad()))
        .WithReportFolder("reports/1_simple_get")
        .Run();
}

await WaitForReady();

// ─── Scenario 2: Deep graph fetch ───────────────────────────────────────────
if (!string.IsNullOrEmpty(Config.OrderId))
{
    Console.WriteLine("\n[Benchmark] ═══ Scenario 2: Deep Graph Fetch ═══");
    using (HttpClient client = CreateClient())
    {
        NBomberRunner
            .RegisterScenarios(
                DeepGraphScenario.RestScenario(client).WithLoadSimulations(ReadLoad()),
                DeepGraphScenario.GraphQLScenario(client).WithLoadSimulations(ReadLoad()))
            .WithReportFolder("reports/2_deep_graph")
            .Run();
    }

    await WaitForReady();
}

// ─── Scenario 3: Over-fetch ─────────────────────────────────────────────────
Console.WriteLine("\n[Benchmark] ═══ Scenario 3: Over-fetch ═══");
using (HttpClient client = CreateClient())
{
    NBomberRunner
        .RegisterScenarios(
            OverFetchScenario.RestScenario(client).WithLoadSimulations(ReadLoad()),
            OverFetchScenario.GraphQLScenario(client).WithLoadSimulations(ReadLoad()))
        .WithReportFolder("reports/3_overfetch")
        .Run();
}

await WaitForReady();

// ─── Scenario 4: N+1 list ───────────────────────────────────────────────────
Console.WriteLine("\n[Benchmark] ═══ Scenario 4: N+1 List ═══");

using (HttpClient probe = new())
{
    string testBody = """{"query":"{ productsWithDetails { id name sku price category { id name } stockItems { supplierId supplier { id name } quantityOnHand } } }"}""";
    using HttpRequestMessage testReq = new(HttpMethod.Post, $"{Config.GraphQLBaseUrl}/graphql");
    testReq.Content = new StringContent(testBody, System.Text.Encoding.UTF8, "application/json");
    using HttpResponseMessage testResp = await probe.SendAsync(testReq);
    string body = await testResp.Content.ReadAsStringAsync();
    Console.WriteLine($"[Benchmark] N+1 GraphQL probe: {testResp.StatusCode} — {body[..Math.Min(300, body.Length)]}");
}

using (HttpClient client = CreateClient())
{
    NBomberRunner
        .RegisterScenarios(
            NPlus1ListScenario.RestScenario(client).WithLoadSimulations(ReadLoad()),
            NPlus1ListScenario.GraphQLScenario(client).WithLoadSimulations(ReadLoad()))
        .WithReportFolder("reports/4_n_plus_1")
        .Run();
}

await WaitForReady();

// ─── Scenario 5: Write + read-back ──────────────────────────────────────────
if (!string.IsNullOrEmpty(Config.CustomerId) && !string.IsNullOrEmpty(Config.SecondProductId))
{
    Console.WriteLine("\n[Benchmark] ═══ Scenario 5: Write + Read-back ═══");
    using HttpClient client = CreateClient();
    NBomberRunner
        .RegisterScenarios(
            WriteReadBackScenario.RestScenario(client).WithLoadSimulations(WriteLoad()),
            WriteReadBackScenario.GraphQLScenario(client).WithLoadSimulations(WriteLoad()))
        .WithReportFolder("reports/5_write_read")
        .Run();
}

Console.WriteLine("\n[Benchmark] All scenarios complete. Reports in ./reports/");
