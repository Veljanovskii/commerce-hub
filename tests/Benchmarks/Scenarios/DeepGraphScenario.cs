using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;

namespace Benchmarks.Scenarios;

public static class DeepGraphScenario
{
    public static ScenarioProps RestScenario(HttpClient client) =>
        Scenario.Create("rest_deep_graph", async context =>
        {
            HttpRequestMessage request = Http.CreateRequest("GET", $"{Config.RestBaseUrl}/orders/{Config.OrderId}");
            return await Http.Send(client, request);
        });

    public static ScenarioProps GraphQLScenario(HttpClient client) =>
        Scenario.Create("graphql_deep_graph", async context =>
        {
            string body = $$"""
                {"query":"{ orderById(id: \"{{Config.OrderId}}\") { id status placedAt total customer { id name email addresses { street city postalCode country } } orderLines { id quantity unitPriceAtOrder product { id name sku price category { id name parentCategory { id name } } } } } }"}
                """;
            HttpRequestMessage request = Http.CreateRequest("POST", $"{Config.GraphQLBaseUrl}/graphql")
                .WithHeader("Content-Type", "application/json")
                .WithBody(new StringContent(body.Trim(), System.Text.Encoding.UTF8, "application/json"));
            return await Http.Send(client, request);
        });
}
