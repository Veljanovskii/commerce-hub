using Domain.Products;
using Domain.Supplies;
using Web.GraphQLApi.DataLoaders;

namespace Web.GraphQLApi.Types;

public class ProductType : ObjectType<Product>
{
    protected override void Configure(IObjectTypeDescriptor<Product> descriptor)
    {
        descriptor.Field(p => p.Id);
        descriptor.Field(p => p.Name);
        descriptor.Field(p => p.Sku);
        descriptor.Field(p => p.Description);
        descriptor.Field(p => p.Price);
        descriptor.Field(p => p.CategoryId);
        descriptor.Field(p => p.Category)
            .Type<CategoryType>()
            .ResolveWith<ProductResolvers>(r => r.GetCategoryAsync(default!, default!, default!));
        descriptor.Field(p => p.StockItems)
            .Type<ListType<StockItemType>>()
            .ResolveWith<ProductResolvers>(r => r.GetStockItemsAsync(default!, default!, default!));
        descriptor.Field(p => p.DomainEvents).Ignore();
    }

    private sealed class ProductResolvers
    {
        public async Task<Category?> GetCategoryAsync(
            [Parent] Product product,
            CategoryByIdDataLoader loader,
            CancellationToken ct) =>
            await loader.LoadAsync(product.CategoryId, ct);

        public async Task<IEnumerable<StockItem>> GetStockItemsAsync(
            [Parent] Product product,
            StockItemsByProductIdDataLoader loader,
            CancellationToken ct) =>
            await loader.LoadAsync(product.Id, ct);
    }
}
