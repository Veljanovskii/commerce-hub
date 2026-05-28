using Application.Abstractions.Messaging;
using Application.Orders.Place;
using SharedKernel;
using Web.RestApi.Extensions;
using Web.RestApi.Infrastructure;

namespace Web.RestApi.Endpoints.Orders;

internal sealed class Place : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("orders", async (
            PlaceOrderRequest request,
            ICommandHandler<PlaceOrderCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new PlaceOrderCommand(
                request.CustomerId,
                request.Lines.Select(l => new PlaceOrderLineItem(l.ProductId, l.Quantity)).ToList());

            Result<Guid> result = await handler.Handle(command, cancellationToken);

            return result.Match(
                onSuccess: orderId => Results.Created($"/orders/{orderId}", new { id = orderId }),
                onFailure: CustomResults.Problem);
        })
        .WithTags("Orders");
    }
}

public sealed record PlaceOrderRequest(Guid CustomerId, List<PlaceOrderLineItemRequest> Lines);

public sealed record PlaceOrderLineItemRequest(Guid ProductId, int Quantity);
