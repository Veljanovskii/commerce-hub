using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.GetById;

public sealed record GetOrderByIdQuery(Guid OrderId) : IQuery<OrderDetailResponse>;

internal sealed class GetOrderByIdQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetOrderByIdQuery, OrderDetailResponse>
{
    public async Task<Result<OrderDetailResponse>> Handle(GetOrderByIdQuery query, CancellationToken cancellationToken)
    {
        OrderDetailResponse? result = await dbContext.Orders
            .AsNoTracking()
            .Where(o => o.Id == query.OrderId)
            .Select(o => new OrderDetailResponse(
                o.Id,
                o.CustomerId,
                o.Customer.Name,
                o.Customer.Email,
                o.Status.ToString(),
                o.PlacedAt,
                o.Total,
                o.OrderLines.Select(l => new OrderLineDto(
                    l.Id,
                    l.ProductId,
                    l.Product.Name,
                    l.Product.Sku,
                    l.Quantity,
                    l.UnitPriceAtOrder)).ToList()))
            .FirstOrDefaultAsync(cancellationToken);

        if (result is null)
        {
            return Result.Failure<OrderDetailResponse>(OrderErrors.NotFound(query.OrderId));
        }

        return result;
    }
}
