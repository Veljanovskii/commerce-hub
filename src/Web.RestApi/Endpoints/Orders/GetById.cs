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
        app.MapGet("api/orders/{orderId:guid}", async (
            Guid orderId,
            IQueryHandler<GetOrderByIdQuery, OrderDetailResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetOrderByIdQuery(orderId);

            Result<OrderDetailResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Orders)
        .RequireAuthorization();
    }
}
