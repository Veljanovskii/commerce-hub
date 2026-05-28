using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.GetHistory;

public sealed record GetProductHistoryQuery(Guid ProductId) : IQuery<List<ProductHistoryEntry>>;

public sealed record ProductHistoryEntry(
    Guid Id,
    string Name,
    string Sku,
    decimal Price,
    DateTime ValidFrom,
    DateTime ValidTo);

internal sealed class GetProductHistoryQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetProductHistoryQuery, List<ProductHistoryEntry>>
{
    public async Task<Result<List<ProductHistoryEntry>>> Handle(
        GetProductHistoryQuery query, CancellationToken cancellationToken)
    {
        List<ProductHistoryEntry> history = await dbContext.Products
            .AsNoTracking()
            .Where(p => p.Id == query.ProductId)
            .Select(p => new ProductHistoryEntry(
                p.Id,
                p.Name,
                p.Sku,
                p.Price,
                p.ValidFrom,
                p.ValidTo))
            .OrderBy(h => h.ValidFrom)
            .ToListAsync(cancellationToken);

        if (history.Count == 0)
        {
            return Result.Failure<List<ProductHistoryEntry>>(ProductErrors.NotFound(query.ProductId));
        }

        return history;
    }
}
