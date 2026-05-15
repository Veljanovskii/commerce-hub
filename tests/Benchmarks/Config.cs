using System.Net.Http.Json;
using System.Text.Json;

namespace Benchmarks;

public static class Config
{
    public static string RestBaseUrl { get; set; } = "http://localhost:5000";
    public static string GraphQLBaseUrl { get; set; } = "http://localhost:5001";
    public static string ProductId { get; set; } = "";
    public static string OrderId { get; set; } = "";
    public static string CustomerId { get; set; } = "";
    public static string SecondProductId { get; set; } = "";

    public static void LoadFromEnv()
    {
        RestBaseUrl = Environment.GetEnvironmentVariable("REST_URL") ?? RestBaseUrl;
        GraphQLBaseUrl = Environment.GetEnvironmentVariable("GRAPHQL_URL") ?? GraphQLBaseUrl;
        ProductId = Environment.GetEnvironmentVariable("BENCHMARK_PRODUCT_ID") ?? ProductId;
        SecondProductId = Environment.GetEnvironmentVariable("BENCHMARK_PRODUCT2_ID") ?? SecondProductId;
        OrderId = Environment.GetEnvironmentVariable("BENCHMARK_ORDER_ID") ?? OrderId;
        CustomerId = Environment.GetEnvironmentVariable("BENCHMARK_CUSTOMER_ID") ?? CustomerId;
    }

    public static async Task DiscoverIdsAsync()
    {
        using HttpClient client = new();

        // Discover products via REST
        if (string.IsNullOrEmpty(ProductId))
        {
            HttpResponseMessage response = await client.GetAsync($"{RestBaseUrl}/products?page=1&pageSize=2");
            response.EnsureSuccessStatusCode();
            using JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            JsonElement items = doc.RootElement.GetProperty("items");
            ProductId = items[0].GetProperty("id").GetString()!;
            if (items.GetArrayLength() > 1)
            {
                SecondProductId = items[1].GetProperty("id").GetString()!;
            }
        }

        // Discover customer + order via GraphQL
        if (string.IsNullOrEmpty(CustomerId) || string.IsNullOrEmpty(OrderId))
        {
            HttpResponseMessage response = await client.PostAsJsonAsync(
                $"{GraphQLBaseUrl}/graphql",
                new { query = "{ products(take: 1) { items { id } } }" });

            // Get first order with customer from GraphQL
            response = await client.PostAsJsonAsync(
                $"{GraphQLBaseUrl}/graphql",
                new
                {
                    query = @"{ products(take: 1) { items { id } } }"
                });

            // We need a list-orders or a known order. For now, place one.
            if (string.IsNullOrEmpty(CustomerId))
            {
                Console.WriteLine("[Benchmark] CustomerId not found. Please set BENCHMARK_CUSTOMER_ID env var.");
            }

            if (string.IsNullOrEmpty(OrderId))
            {
                Console.WriteLine("[Benchmark] OrderId not found. Please set BENCHMARK_ORDER_ID env var.");
            }
        }

        Console.WriteLine($"[Benchmark] REST={RestBaseUrl} GraphQL={GraphQLBaseUrl}");
        Console.WriteLine($"[Benchmark] ProductId={ProductId} OrderId={OrderId} CustomerId={CustomerId}");
    }
}
