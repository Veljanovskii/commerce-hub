using Domain.Supplies;
using Web.GraphQLApi.DataLoaders;

namespace Web.GraphQLApi.Types;

public class StockItemType : ObjectType<StockItem>
{
    protected override void Configure(IObjectTypeDescriptor<StockItem> descriptor)
    {
        descriptor.Field(s => s.Id);
        descriptor.Field(s => s.ProductId);
        descriptor.Field(s => s.SupplierId);
        descriptor.Field(s => s.Supplier)
            .Type<SupplierType>()
            .ResolveWith<StockItemResolvers>(r => r.GetSupplierAsync(default!, default!, default!));
        descriptor.Field(s => s.QuantityOnHand);
        descriptor.Field(s => s.ReorderLevel);
        descriptor.Field(s => s.Product).Ignore();
        descriptor.Field(s => s.DomainEvents).Ignore();
    }

    private sealed class StockItemResolvers
    {
        public async Task<Supplier?> GetSupplierAsync(
            [Parent] StockItem stockItem,
            SupplierByIdDataLoader loader,
            CancellationToken ct) =>
            await loader.LoadAsync(stockItem.SupplierId, ct);
    }
}
