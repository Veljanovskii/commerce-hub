using Application.Abstractions.Messaging;
using Application.Catalog.Products;
using Application.Catalog.Products.GetById;
using SharedKernel;
using Web.RestApi.Extensions;
using Web.RestApi.Infrastructure;

namespace Web.RestApi.Endpoints.Products;

internal sealed class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/products/{productId:guid}", async (
            Guid productId,
            IQueryHandler<GetProductByIdQuery, ProductDetailResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetProductByIdQuery(productId);

            Result<ProductDetailResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Products)
        .RequireAuthorization();
    }
}
