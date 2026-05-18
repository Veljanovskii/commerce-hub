using Application.Abstractions.Messaging;
using Application.Catalog.Categories;
using Application.Catalog.Categories.GetList;
using SharedKernel;
using Web.RestApi.Extensions;
using Web.RestApi.Infrastructure;

namespace Web.RestApi.Endpoints.Categories;

internal sealed class GetList : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/categories", async (
            IQueryHandler<GetCategoriesQuery, List<CategoryResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetCategoriesQuery();

            Result<List<CategoryResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Categories)
        .RequireAuthorization();
    }
}
