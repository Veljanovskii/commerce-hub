using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;

namespace Benchmarks.Scenarios;

public static class OverFetchScenario
{
    public static ScenarioProps RestScenario(HttpClient client) =>
        Scenario.Create("rest_overfetch", async context =>
        {
            // REST returns the full product DTO even though we only need name + price
            HttpRequestMessage request = Http.CreateRequest("GET", $"{Config.RestBaseUrl}/products/{Config.ProductId}");
            return await Http.Send(client, request);
        });

    public static ScenarioProps GraphQLScenario(HttpClient client) =>
        Scenario.Create("graphql_minimal_fetch", async context =>
        {
            // GraphQL fetches only the two fields we need
            string body = $$"""{"query":"{ productById(id: \"{{Config.ProductId}}\") { name price } }"}""";
            HttpRequestMessage request = Http.CreateRequest("POST", $"{Config.GraphQLBaseUrl}/graphql")
                .WithHeader("Content-Type", "application/json")
                .WithBody(new StringContent(body, System.Text.Encoding.UTF8, "application/json"));
            return await Http.Send(client, request);
        });
}
