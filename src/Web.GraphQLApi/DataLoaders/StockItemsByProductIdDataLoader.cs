using Application.Abstractions.Data;
using Domain.Supplies;
using Microsoft.EntityFrameworkCore;

namespace Web.GraphQLApi.DataLoaders;

public sealed class StockItemsByProductIdDataLoader(
    IServiceProvider serviceProvider,
    IBatchScheduler batchScheduler,
    DataLoaderOptions? options = null)
    : GroupedDataLoader<Guid, StockItem>(batchScheduler, options ?? new DataLoaderOptions())
{
    protected override async Task<ILookup<Guid, StockItem>> LoadGroupedBatchAsync(
        IReadOnlyList<Guid> keys, CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
        IApplicationDbContext db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        List<StockItem> items = await db.StockItems
            .AsNoTracking()
            .Include(s => s.Supplier)
            .Where(s => keys.Contains(s.ProductId))
            .ToListAsync(cancellationToken);

        return items.ToLookup(s => s.ProductId);
    }
}
