using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;

namespace Benchmarks.Scenarios;

public static class ProductListScenario
{
    public static ScenarioProps RestScenario(HttpClient client) =>
        Scenario.Create("rest_product_list", async context =>
        {
            HttpRequestMessage request = Http.CreateRequest("GET", $"{Config.RestBaseUrl}/products/list?page=1&pageSize=50");
            return await Http.Send(client, request);
        });

    public static ScenarioProps GraphQLScenario(HttpClient client) =>
        Scenario.Create("graphql_product_list", async context =>
        {
            string body = """
            {
              "query": "{ productsWithDetails(page: 1, pageSize: 50) { id name sku description price categoryId category { name } } }"
            }
            """;

            HttpRequestMessage request = Http.CreateRequest("POST", $"{Config.GraphQLBaseUrl}/graphql")
                .WithHeader("Content-Type", "application/json")
                .WithBody(new StringContent(body, System.Text.Encoding.UTF8, "application/json"));

            return await Http.Send(client, request);
        });
}
