using Application.Abstractions.Messaging;
using Application.Orders.UpdateStatus;
using Domain.Orders;
using SharedKernel;
using Web.RestApi.Extensions;
using Web.RestApi.Infrastructure;

namespace Web.RestApi.Endpoints.Orders;

internal sealed class UpdateStatus : IEndpoint
{
    public sealed class Request
    {
        public OrderStatus NewStatus { get; set; }
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("api/orders/{orderId:guid}/status", async (
            Guid orderId,
            Request request,
            ICommandHandler<UpdateOrderStatusCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateOrderStatusCommand
            {
                OrderId = orderId,
                NewStatus = request.NewStatus
            };

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Orders)
        .RequireAuthorization();
    }
}
