using Application.Abstractions.Messaging;
using Application.Products.GetHistory;
using SharedKernel;
using Web.RestApi.Extensions;
using Web.RestApi.Infrastructure;

namespace Web.RestApi.Endpoints.Products;

internal sealed class GetHistory : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("products/{id:guid}/history", async (
            Guid id,
            IQueryHandler<GetProductHistoryQuery, List<ProductHistoryEntry>> handler,
            CancellationToken cancellationToken) =>
        {
            Result<List<ProductHistoryEntry>> result = await handler.Handle(
                new GetProductHistoryQuery(id), cancellationToken);

            return result.Match(
                onSuccess: history => Results.Ok(history),
                onFailure: CustomResults.Problem);
        })
        .WithTags("Products");
    }
}
