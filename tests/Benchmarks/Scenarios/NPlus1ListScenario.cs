using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;

namespace Benchmarks.Scenarios;

public static class NPlus1ListScenario
{
    public static ScenarioProps RestScenario(HttpClient client) =>
        Scenario.Create("rest_n_plus_1_list", async context =>
        {
            // REST: get 50 products (each with category name from the flat DTO)
            HttpRequestMessage request = Http.CreateRequest("GET", $"{Config.RestBaseUrl}/products?page=1&pageSize=50");
            return await Http.Send(client, request);
        });

    public static ScenarioProps GraphQLScenario(HttpClient client) =>
        Scenario.Create("graphql_n_plus_1_list", async context =>
        {
            // GraphQL: get all products with nested category and stock items with supplier (uses DataLoaders)
            string body = """{"query":"{ productsWithDetails { id name sku price category { id name } stockItems { supplierId supplier { id name } quantityOnHand } } }"}""";
            HttpRequestMessage request = Http.CreateRequest("POST", $"{Config.GraphQLBaseUrl}/graphql")
                .WithHeader("Content-Type", "application/json")
                .WithBody(new StringContent(body, System.Text.Encoding.UTF8, "application/json"));
            return await Http.Send(client, request);
        });
}
