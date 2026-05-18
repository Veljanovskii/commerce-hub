using Application.Abstractions.Messaging;
using Application.Catalog.Products;
using Application.Catalog.Products.Create;
using SharedKernel;
using Web.RestApi.Extensions;
using Web.RestApi.Infrastructure;

namespace Web.RestApi.Endpoints.Products;

internal sealed class Create : IEndpoint
{
    public sealed class Request
    {
        public string Name { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int StockQuantity { get; set; }
        public Guid CategoryId { get; set; }
        public Guid SupplierId { get; set; }
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/products", async (
            Request request,
            ICommandHandler<CreateProductCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateProductCommand
            {
                Name = request.Name,
                Sku = request.Sku,
                Description = request.Description,
                UnitPrice = request.UnitPrice,
                StockQuantity = request.StockQuantity,
                CategoryId = request.CategoryId,
                SupplierId = request.SupplierId
            };

            Result<Guid> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Products)
        .RequireAuthorization();
    }
}
