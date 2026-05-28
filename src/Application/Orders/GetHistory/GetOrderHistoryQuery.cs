using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.GetHistory;

public sealed record GetOrderHistoryQuery(Guid OrderId) : IQuery<List<OrderHistoryEntry>>;

public sealed record OrderHistoryEntry(
    Guid Id,
    string Status,
    decimal Total,
    DateTime ValidFrom,
    DateTime ValidTo);

internal sealed class GetOrderHistoryQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetOrderHistoryQuery, List<OrderHistoryEntry>>
{
    public async Task<Result<List<OrderHistoryEntry>>> Handle(
        GetOrderHistoryQuery query, CancellationToken cancellationToken)
    {
        List<OrderHistoryEntry> history = await dbContext.Orders
            .AsNoTracking()
            .Where(o => o.Id == query.OrderId)
            .Select(o => new OrderHistoryEntry(
                o.Id,
                o.Status.ToString(),
                o.Total,
                o.ValidFrom,
                o.ValidTo))
            .OrderBy(h => h.ValidFrom)
            .ToListAsync(cancellationToken);

        if (history.Count == 0)
        {
            return Result.Failure<List<OrderHistoryEntry>>(OrderErrors.NotFound(query.OrderId));
        }

        return history;
    }
}
