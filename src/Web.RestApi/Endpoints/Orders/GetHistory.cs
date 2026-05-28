using Application.Abstractions.Messaging;
using Application.Orders.GetHistory;
using SharedKernel;
using Web.RestApi.Extensions;
using Web.RestApi.Infrastructure;

namespace Web.RestApi.Endpoints.Orders;

internal sealed class GetHistory : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("orders/{id:guid}/history", async (
            Guid id,
            IQueryHandler<GetOrderHistoryQuery, List<OrderHistoryEntry>> handler,
            CancellationToken cancellationToken) =>
        {
            Result<List<OrderHistoryEntry>> result = await handler.Handle(
                new GetOrderHistoryQuery(id), cancellationToken);

            return result.Match(
                onSuccess: history => Results.Ok(history),
                onFailure: CustomResults.Problem);
        })
        .WithTags("Orders");
    }
}
