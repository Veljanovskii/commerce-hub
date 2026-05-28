using Domain.Orders;
using Domain.Products;
using Web.GraphQLApi.DataLoaders;

namespace Web.GraphQLApi.Types;

public class OrderLineType : ObjectType<OrderLine>
{
    protected override void Configure(IObjectTypeDescriptor<OrderLine> descriptor)
    {
        descriptor.Field(l => l.Id);
        descriptor.Field(l => l.OrderId);
        descriptor.Field(l => l.ProductId);
        descriptor.Field(l => l.Product)
            .Type<ProductType>()
            .ResolveWith<OrderLineResolvers>(r => r.GetProductAsync(default!, default!, default!));
        descriptor.Field(l => l.Quantity);
        descriptor.Field(l => l.UnitPriceAtOrder);
        descriptor.Field(l => l.Order).Ignore();
    }

    private sealed class OrderLineResolvers
    {
        public async Task<Product?> GetProductAsync(
            [Parent] OrderLine line,
            ProductByIdDataLoader loader,
            CancellationToken ct) =>
            await loader.LoadAsync(line.ProductId, ct);
    }
}
