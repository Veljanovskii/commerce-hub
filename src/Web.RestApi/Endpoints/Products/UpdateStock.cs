using Application.Abstractions.Messaging;
using Application.Catalog.Products.UpdateStock;
using SharedKernel;
using Web.RestApi.Extensions;
using Web.RestApi.Infrastructure;

namespace Web.RestApi.Endpoints.Products;

internal sealed class UpdateStock : IEndpoint
{
    public sealed class Request
    {
        public int Quantity { get; set; }
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("api/products/{productId:guid}/stock", async (
            Guid productId,
            Request request,
            ICommandHandler<UpdateStockCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateStockCommand
            {
                ProductId = productId,
                Quantity = request.Quantity
            };

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Products)
        .RequireAuthorization();
    }
}
