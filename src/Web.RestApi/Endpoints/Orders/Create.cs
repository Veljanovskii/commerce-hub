using Application.Abstractions.Messaging;
using Application.Orders;
using Application.Orders.Create;
using SharedKernel;
using Web.RestApi.Extensions;
using Web.RestApi.Infrastructure;

namespace Web.RestApi.Endpoints.Orders;

internal sealed class Create : IEndpoint
{
    public sealed class Request
    {
        public Guid CustomerId { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string ShippingCity { get; set; } = string.Empty;
        public string ShippingCountry { get; set; } = string.Empty;
        public List<OrderItemRequest> Items { get; set; } = [];
    }

    public sealed class OrderItemRequest
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Discount { get; set; }
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/orders", async (
            Request request,
            ICommandHandler<CreateOrderCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateOrderCommand
            {
                CustomerId = request.CustomerId,
                ShippingAddress = request.ShippingAddress,
                ShippingCity = request.ShippingCity,
                ShippingCountry = request.ShippingCountry,
                Items = request.Items.Select(i => new CreateOrderItemDto
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Discount = i.Discount
                }).ToList()
            };

            Result<Guid> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Orders)
        .RequireAuthorization();
    }
}
