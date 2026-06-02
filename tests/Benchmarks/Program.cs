using Benchmarks;
using Benchmarks.Scenarios;
using NBomber.Contracts;
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
const int warmupDuration = 15;

LoadSimulation[] ReadLoad() =>
[
    Simulation.RampingInject(rate: readRate, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(rampDuration)),
    Simulation.Inject(rate: readRate, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(sustainDuration))
];

LoadSimulation[] WriteLoad() =>
[
    Simulation.RampingInject(rate: writeRate, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(rampDuration)),
    Simulation.Inject(rate: writeRate, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(sustainDuration))
];

LoadSimulation[] WarmupLoad() =>
[
    Simulation.Inject(rate: readRate, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(warmupDuration))
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

async Task RunIsolatedAsync(
    string reportFolder,
    Func<HttpClient, ScenarioProps> restScenario,
    Func<HttpClient, ScenarioProps> graphQLScenario,
    LoadSimulation[] load)
{
    using (HttpClient restClient = CreateClient())
    {
        NBomberRunner
            .RegisterScenarios(restScenario(restClient).WithLoadSimulations(load))
            .WithReportFolder($"{reportFolder}/rest")
            .Run();
    }

    await WaitForReady();

    using HttpClient graphQLClient = CreateClient();
    NBomberRunner
        .RegisterScenarios(graphQLScenario(graphQLClient).WithLoadSimulations(load))
        .WithReportFolder($"{reportFolder}/graphql")
        .Run();
}

// ─── Warmup (discarded) ─────────────────────────────────────────────────────
// Drives both APIs before any measured scenario to absorb cold-start JIT compilation
// and EF Core query-plan compilation. Reports are disabled so these numbers never count.
Console.WriteLine("\n[Benchmark] ═══ Warmup (discarded) ═══");
using (HttpClient warmupClient = CreateClient())
{
    NBomberRunner
        .RegisterScenarios(SimpleGetScenario.RestScenario(warmupClient).WithLoadSimulations(WarmupLoad()))
        .WithoutReports()
        .Run();
}

using (HttpClient warmupClient = CreateClient())
{
    NBomberRunner
        .RegisterScenarios(SimpleGetScenario.GraphQLScenario(warmupClient).WithLoadSimulations(WarmupLoad()))
        .WithoutReports()
        .Run();
}

await WaitForReady();

// ─── Scenario 1: Simple GET ─────────────────────────────────────────────────
Console.WriteLine("\n[Benchmark] ═══ Scenario 1: Simple GET ═══");
await RunIsolatedAsync(
    "reports/1_simple_get",
    SimpleGetScenario.RestScenario,
    SimpleGetScenario.GraphQLScenario,
    ReadLoad());

await WaitForReady();

// ─── Scenario 2: Deep graph fetch ───────────────────────────────────────────
if (!string.IsNullOrEmpty(Config.OrderId))
{
    Console.WriteLine("\n[Benchmark] ═══ Scenario 2: Deep Graph Fetch ═══");
    await RunIsolatedAsync(
        "reports/2_deep_graph",
        DeepGraphScenario.RestScenario,
        DeepGraphScenario.GraphQLScenario,
        ReadLoad());

    await WaitForReady();
}

// ─── Scenario 3: Over-fetch ─────────────────────────────────────────────────
Console.WriteLine("\n[Benchmark] ═══ Scenario 3: Over-fetch ═══");
await RunIsolatedAsync(
    "reports/3_overfetch",
    OverFetchScenario.RestScenario,
    OverFetchScenario.GraphQLScenario,
    ReadLoad());

await WaitForReady();

// ─── Scenario 4: Product list ───────────────────────────────────────────────
Console.WriteLine("\n[Benchmark] ═══ Scenario 4: Product List ═══");
await RunIsolatedAsync(
    "reports/4_product_list",
    ProductListScenario.RestScenario,
    ProductListScenario.GraphQLScenario,
    ReadLoad());

await WaitForReady();

// ─── Scenario 5: Write + read-back ──────────────────────────────────────────
if (!string.IsNullOrEmpty(Config.CustomerId) && !string.IsNullOrEmpty(Config.SecondProductId))
{
    Console.WriteLine("\n[Benchmark] ═══ Scenario 5: Write + Read-back ═══");
    await RunIsolatedAsync(
        "reports/5_write_read",
        WriteReadBackScenario.RestScenario,
        WriteReadBackScenario.GraphQLScenario,
        WriteLoad());
}

Console.WriteLine("\n[Benchmark] All scenarios complete. Reports in ./reports/");
