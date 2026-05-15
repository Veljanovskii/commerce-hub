using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.GetStatus;

public sealed record GetOrderStatusQuery(Guid OrderId) : IQuery<OrderStatusResponse>;

public sealed record OrderStatusResponse(Guid OrderId, string Status);

internal sealed class GetOrderStatusQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetOrderStatusQuery, OrderStatusResponse>
{
    public async Task<Result<OrderStatusResponse>> Handle(GetOrderStatusQuery query, CancellationToken cancellationToken)
    {
        OrderStatusResponse? result = await dbContext.Orders
            .AsNoTracking()
            .Where(o => o.Id == query.OrderId)
            .Select(o => new OrderStatusResponse(o.Id, o.Status.ToString()))
            .FirstOrDefaultAsync(cancellationToken);

        if (result is null)
        {
            return Result.Failure<OrderStatusResponse>(OrderErrors.NotFound(query.OrderId));
        }

        return result;
    }
}
