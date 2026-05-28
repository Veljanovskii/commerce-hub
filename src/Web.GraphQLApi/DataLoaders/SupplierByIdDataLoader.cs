using Application.Abstractions.Data;
using Domain.Supplies;
using Microsoft.EntityFrameworkCore;

namespace Web.GraphQLApi.DataLoaders;

public sealed class SupplierByIdDataLoader(
    IServiceProvider serviceProvider,
    IBatchScheduler batchScheduler,
    DataLoaderOptions? options = null)
    : BatchDataLoader<Guid, Supplier>(batchScheduler, options ?? new DataLoaderOptions())
{
    protected override async Task<IReadOnlyDictionary<Guid, Supplier>> LoadBatchAsync(
        IReadOnlyList<Guid> keys, CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
        IApplicationDbContext db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        List<Supplier> suppliers = await db.Suppliers
            .AsNoTracking()
            .Where(s => keys.Contains(s.Id))
            .ToListAsync(cancellationToken);

        return suppliers.ToDictionary(s => s.Id);
    }
}
