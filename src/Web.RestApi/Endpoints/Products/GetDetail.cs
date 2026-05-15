using Application.Abstractions.Messaging;
using Application.Products;
using Application.Products.GetDetail;
using SharedKernel;
using Web.RestApi.Extensions;
using Web.RestApi.Infrastructure;

namespace Web.RestApi.Endpoints.Products;

internal sealed class GetDetail : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("products/{id:guid}/detail", async (
            Guid id,
            IQueryHandler<GetProductDetailQuery, ProductDetailResponse> handler,
            CancellationToken cancellationToken) =>
        {
            Result<ProductDetailResponse> result = await handler.Handle(new GetProductDetailQuery(id), cancellationToken);

            return result.Match(
                onSuccess: product => Results.Ok(product),
                onFailure: CustomResults.Problem);
        })
        .WithTags("Products");
    }
}
