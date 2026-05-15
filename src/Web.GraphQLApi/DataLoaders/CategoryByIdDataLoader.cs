using Application.Abstractions.Data;
using Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Web.GraphQLApi.DataLoaders;

public sealed class CategoryByIdDataLoader(
    IServiceProvider serviceProvider,
    IBatchScheduler batchScheduler,
    DataLoaderOptions? options = null)
    : BatchDataLoader<Guid, Category>(batchScheduler, options ?? new DataLoaderOptions())
{
    protected override async Task<IReadOnlyDictionary<Guid, Category>> LoadBatchAsync(
        IReadOnlyList<Guid> keys, CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
        IApplicationDbContext db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        List<Category> categories = await db.Categories
            .AsNoTracking()
            .Include(c => c.ParentCategory)
            .Where(c => keys.Contains(c.Id))
            .ToListAsync(cancellationToken);

        return categories.ToDictionary(c => c.Id);
    }
}
