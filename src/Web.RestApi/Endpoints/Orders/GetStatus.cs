using Application.Abstractions.Messaging;
using Application.Orders.GetStatus;
using SharedKernel;
using Web.RestApi.Extensions;
using Web.RestApi.Infrastructure;

namespace Web.RestApi.Endpoints.Orders;

internal sealed class GetStatus : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("orders/{id:guid}/status", async (
            Guid id,
            IQueryHandler<GetOrderStatusQuery, OrderStatusResponse> handler,
            CancellationToken cancellationToken) =>
        {
            Result<OrderStatusResponse> result = await handler.Handle(new GetOrderStatusQuery(id), cancellationToken);

            return result.Match(
                onSuccess: status => Results.Ok(status),
                onFailure: CustomResults.Problem);
        })
        .WithTags("Orders");
    }
}
