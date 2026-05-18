using Application.Abstractions.Messaging;
using Application.Catalog.Categories;
using Application.Catalog.Categories.GetById;
using SharedKernel;
using Web.RestApi.Extensions;
using Web.RestApi.Infrastructure;

namespace Web.RestApi.Endpoints.Categories;

internal sealed class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/categories/{categoryId:guid}", async (
            Guid categoryId,
            IQueryHandler<GetCategoryByIdQuery, CategoryResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetCategoryByIdQuery(categoryId);

            Result<CategoryResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Categories)
        .RequireAuthorization();
    }
}
