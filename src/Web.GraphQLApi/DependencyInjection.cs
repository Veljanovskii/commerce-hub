using Web.GraphQLApi.Infrastructure;

namespace Web.GraphQLApi;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services
            .AddGraphQLServer()
            .AddQueryType()
            .AddMutationType()
            .AddTypeExtension<Queries.ProductQueries>()
            .AddTypeExtension<Queries.CategoryQueries>()
            .AddTypeExtension<Queries.SupplierQueries>()
            .AddTypeExtension<Queries.OrderQueries>()
            .AddTypeExtension<Queries.CustomerQueries>()
            .AddTypeExtension<Queries.DashboardQueries>()
            .AddTypeExtension<Mutations.ProductMutations>()
            .AddTypeExtension<Mutations.CategoryMutations>()
            .AddTypeExtension<Mutations.SupplierMutations>()
            .AddTypeExtension<Mutations.OrderMutations>()
            .AddTypeExtension<Mutations.CustomerMutations>();

        services.AddAuthorization();

        return services;
    }
}
