using Application.Abstractions.Messaging;
using Application.Products;
using Application.Products.GetAll;
using SharedKernel;
using Web.RestApi.Extensions;
using Web.RestApi.Infrastructure;

namespace Web.RestApi.Endpoints.Products;

internal sealed class GetAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("products", async (
            Guid? categoryId,
            int? page,
            int? pageSize,
            IQueryHandler<GetProductsQuery, PagedResponse<ProductResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetProductsQuery(categoryId, page ?? 1, pageSize ?? 20);

            Result<PagedResponse<ProductResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(
                onSuccess: products => Results.Ok(products),
                onFailure: CustomResults.Problem);
        })
        .WithTags("Products");
    }
}
