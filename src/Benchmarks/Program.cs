using System.Text;
using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;

#pragma warning disable S1075 // URIs should not be hardcoded

namespace Benchmarks;

public static class Program
{
    public static void Main()
    {
        using var httpClient = new HttpClient();
        
        // Ensure the API is running via Aspire AppHost before running benchmarks
        string restBaseUrl = "http://localhost:5101/api";
        string graphqlBaseUrl = "http://localhost:5102/graphql";

        // Scenario: REST Get Products List
        ScenarioProps restGetProductsScenario = Scenario.Create("rest_get_products", async context =>
        {
            HttpRequestMessage request = Http.CreateRequest("GET", $"{restBaseUrl}/products?page=1&pageSize=50");
            Response<HttpResponseMessage> response = await Http.Send(httpClient, request);
            return response;
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 50, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        // Scenario: GraphQL Get Products List
        ScenarioProps graphqlGetProductsScenario = Scenario.Create("graphql_get_products", async context =>
        {
            string query = @"{""query"":""query { products(page: 1, pageSize: 50) { id name unitPrice stockQuantity categoryName supplierName } }""}";
            
            HttpRequestMessage request = Http.CreateRequest("POST", graphqlBaseUrl)
                .WithHeader("Accept", "application/json")
                .WithBody(new StringContent(query, Encoding.UTF8, "application/json"));

            Response<HttpResponseMessage> response = await Http.Send(httpClient, request);
            return response;
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 50, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        // Scenario: REST Dashboard Aggregation (S6)
        ScenarioProps restDashboardScenario = Scenario.Create("rest_dashboard", async context =>
        {
            HttpRequestMessage request = Http.CreateRequest("GET", $"{restBaseUrl}/orders/dashboard");
            Response<HttpResponseMessage> response = await Http.Send(httpClient, request);
            return response;
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 20, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        // Scenario: GraphQL Dashboard Aggregation (S6)
        ScenarioProps graphqlDashboardScenario = Scenario.Create("graphql_dashboard", async context =>
        {
            string query = @"{""query"":""query { dashboard { totalRevenue totalOrders totalProducts totalCustomers recentOrders { orderId customerName totalAmount status } topProducts { productName totalQuantitySold totalRevenue } lowStockProducts { productName sku stockQuantity } } }""}";
            
            HttpRequestMessage request = Http.CreateRequest("POST", graphqlBaseUrl)
                .WithHeader("Accept", "application/json")
                .WithBody(new StringContent(query, Encoding.UTF8, "application/json"));

            Response<HttpResponseMessage> response = await Http.Send(httpClient, request);
            return response;
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 20, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        NBomberRunner
            .RegisterScenarios(
                restGetProductsScenario, 
                graphqlGetProductsScenario,
                restDashboardScenario,
                graphqlDashboardScenario)
            .WithReportFolder("reports")
            .Run();
    }
}
