using Application;
using HealthChecks.UI.Client;
using Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Web.GraphQLApi;
using Web.GraphQLApi.Types;
using Web.Shared;
using Web.Shared.Interceptors;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddCommerceHubObservability("CommerceHub.GraphQLApi");

builder.Services.AddSingleton<IInterceptor, BackendCallCountingInterceptor>();

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddSubscriptionType<Subscription>()
    .AddType<ProductType>()
    .AddType<CategoryType>()
    .AddType<OrderType>()
    .AddType<OrderLineType>()
    .AddType<CustomerType>()
    .AddType<StockItemType>()
    .AddType<SupplierType>()
    .AddDataLoader<Web.GraphQLApi.DataLoaders.CategoryByIdDataLoader>()
    .AddDataLoader<Web.GraphQLApi.DataLoaders.SupplierByIdDataLoader>()
    .AddDataLoader<Web.GraphQLApi.DataLoaders.ProductByIdDataLoader>()
    .AddDataLoader<Web.GraphQLApi.DataLoaders.StockItemsByProductIdDataLoader>()
    .AddFiltering()
    .AddSorting()
    .AddProjections()
    .AddInMemorySubscriptions();

WebApplication app = builder.Build();

app.MapDefaultEndpoints();

app.MapGraphQL()
    .WithOptions(o => o.Tool.Enable = app.Environment.IsDevelopment());

app.UseWebSockets();

app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

await app.RunAsync();
