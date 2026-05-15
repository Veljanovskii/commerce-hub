using Application.Abstractions.Messaging;
using Application.Products;
using Application.Products.GetById;
using SharedKernel;
using Web.RestApi.Extensions;
using Web.RestApi.Infrastructure;

namespace Web.RestApi.Endpoints.Products;

internal sealed class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("products/{id:guid}", async (
            Guid id,
            IQueryHandler<GetProductByIdQuery, ProductResponse> handler,
            CancellationToken cancellationToken) =>
        {
            Result<ProductResponse> result = await handler.Handle(new GetProductByIdQuery(id), cancellationToken);

            return result.Match(
                onSuccess: product => Results.Ok(product),
                onFailure: CustomResults.Problem);
        })
        .WithTags("Products");
    }
}
