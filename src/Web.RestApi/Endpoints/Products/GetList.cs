using Application.Abstractions.Messaging;
using Application.Products;
using Application.Products.GetList;
using SharedKernel;
using Web.RestApi.Extensions;
using Web.RestApi.Infrastructure;

namespace Web.RestApi.Endpoints.Products;

internal sealed class GetList : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("products/list", async (
            Guid? categoryId,
            int? page,
            int? pageSize,
            IQueryHandler<GetProductListQuery, List<ProductResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetProductListQuery(
                categoryId,
                page ?? 1,
                pageSize ?? 20);

            Result<List<ProductResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(
                onSuccess: products => Results.Ok(products),
                onFailure: CustomResults.Problem);
        })
        .WithTags("Products");
    }
}
