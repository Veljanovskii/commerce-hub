using System.Text.Json;
using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;

namespace Benchmarks.Scenarios;

public static class WriteReadBackScenario
{
    public static ScenarioProps RestScenario(HttpClient client) =>
        Scenario.Create("rest_write_read", async context =>
        {
            // Place order
            string orderBody = JsonSerializer.Serialize(new
            {
                customerId = Config.CustomerId,
                lines = new[]
                {
                    new { productId = Config.ProductId, quantity = 1 },
                    new { productId = Config.SecondProductId, quantity = 2 }
                }
            });
            HttpRequestMessage placeRequest = Http.CreateRequest("POST", $"{Config.RestBaseUrl}/orders")
                .WithHeader("Content-Type", "application/json")
                .WithBody(new StringContent(orderBody, System.Text.Encoding.UTF8, "application/json"));
            Response<HttpResponseMessage> placeResponse = await Http.Send(client, placeRequest);

            if (!placeResponse.IsError)
            {
                string responseBody = await placeResponse.Payload.Value.Content.ReadAsStringAsync();
                using JsonDocument doc = JsonDocument.Parse(responseBody);
                string orderId = doc.RootElement.GetProperty("id").GetString()!;

                // Read back
                HttpRequestMessage readRequest = Http.CreateRequest("GET", $"{Config.RestBaseUrl}/orders/{orderId}");
                return await Http.Send(client, readRequest);
            }

            return placeResponse;
        });

    public static ScenarioProps GraphQLScenario(HttpClient client) =>
        Scenario.Create("graphql_write_read", async context =>
        {
            // Place order mutation
            string mutation = $$"""
                {"query":"mutation { placeOrder(input: { customerId: \"{{Config.CustomerId}}\", lines: [{ productId: \"{{Config.ProductId}}\", quantity: 1 }, { productId: \"{{Config.SecondProductId}}\", quantity: 2 }] }) { id status total orderLines { productId quantity unitPriceAtOrder } } }"}
                """;
            HttpRequestMessage placeRequest = Http.CreateRequest("POST", $"{Config.GraphQLBaseUrl}/graphql")
                .WithHeader("Content-Type", "application/json")
                .WithBody(new StringContent(mutation.Trim(), System.Text.Encoding.UTF8, "application/json"));
            return await Http.Send(client, placeRequest);
        });
}
