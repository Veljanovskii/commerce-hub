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

using HttpClient httpClient = new();

NBomber.Contracts.LoadSimulation[] loadSteps =
[
    Simulation.RampingInject(rate: 50, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30)),
    Simulation.Inject(rate: 50, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(60))
];

// Scenario 1: Simple GET
NBomberRunner
    .RegisterScenarios(
        SimpleGetScenario.RestScenario(httpClient).WithLoadSimulations(loadSteps),
        SimpleGetScenario.GraphQLScenario(httpClient).WithLoadSimulations(loadSteps))
    .WithReportFolder("reports/1_simple_get")
    .Run();

// Scenario 2: Deep graph fetch
if (!string.IsNullOrEmpty(Config.OrderId))
{
    NBomberRunner
        .RegisterScenarios(
            DeepGraphScenario.RestScenario(httpClient).WithLoadSimulations(loadSteps),
            DeepGraphScenario.GraphQLScenario(httpClient).WithLoadSimulations(loadSteps))
        .WithReportFolder("reports/2_deep_graph")
        .Run();
}

// Scenario 3: Over-fetch
NBomberRunner
    .RegisterScenarios(
        OverFetchScenario.RestScenario(httpClient).WithLoadSimulations(loadSteps),
        OverFetchScenario.GraphQLScenario(httpClient).WithLoadSimulations(loadSteps))
    .WithReportFolder("reports/3_overfetch")
    .Run();

// Scenario 4: N+1 list
NBomberRunner
    .RegisterScenarios(
        NPlus1ListScenario.RestScenario(httpClient).WithLoadSimulations(loadSteps),
        NPlus1ListScenario.GraphQLScenario(httpClient).WithLoadSimulations(loadSteps))
    .WithReportFolder("reports/4_n_plus_1")
    .Run();

// Scenario 5: Write + read-back
if (!string.IsNullOrEmpty(Config.CustomerId) && !string.IsNullOrEmpty(Config.SecondProductId))
{
    NBomberRunner
        .RegisterScenarios(
            WriteReadBackScenario.RestScenario(httpClient).WithLoadSimulations(loadSteps),
            WriteReadBackScenario.GraphQLScenario(httpClient).WithLoadSimulations(loadSteps))
        .WithReportFolder("reports/5_write_read")
        .Run();
}

Console.WriteLine("[Benchmark] All scenarios complete. Reports in ./reports/");
