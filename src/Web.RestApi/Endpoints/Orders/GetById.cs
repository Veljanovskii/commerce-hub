using Application.Abstractions.Messaging;
using Application.Orders;
using Application.Orders.GetById;
using SharedKernel;
using Web.RestApi.Extensions;
using Web.RestApi.Infrastructure;

namespace Web.RestApi.Endpoints.Orders;

internal sealed class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("orders/{id:guid}", async (
            Guid id,
            IQueryHandler<GetOrderByIdQuery, OrderDetailResponse> handler,
            CancellationToken cancellationToken) =>
        {
            Result<OrderDetailResponse> result = await handler.Handle(new GetOrderByIdQuery(id), cancellationToken);

            return result.Match(
                onSuccess: order => Results.Ok(order),
                onFailure: CustomResults.Problem);
        })
        .WithTags("Orders");
    }
}
