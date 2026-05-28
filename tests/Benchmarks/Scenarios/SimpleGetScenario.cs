using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;

namespace Benchmarks.Scenarios;

public static class SimpleGetScenario
{
    public static ScenarioProps RestScenario(HttpClient client) =>
        Scenario.Create("rest_simple_get", async context =>
        {
            HttpRequestMessage request = Http.CreateRequest("GET", $"{Config.RestBaseUrl}/products/{Config.ProductId}");
            return await Http.Send(client, request);
        });

    public static ScenarioProps GraphQLScenario(HttpClient client) =>
        Scenario.Create("graphql_simple_get", async context =>
        {
            string body = $$"""{"query":"{ productById(id: \"{{Config.ProductId}}\") { id name sku description price categoryId } }"}""";
            HttpRequestMessage request = Http.CreateRequest("POST", $"{Config.GraphQLBaseUrl}/graphql")
                .WithHeader("Content-Type", "application/json")
                .WithBody(new StringContent(body, System.Text.Encoding.UTF8, "application/json"));
            return await Http.Send(client, request);
        });
}
