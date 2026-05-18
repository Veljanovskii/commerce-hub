using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.GetList;

internal sealed class GetOrdersQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetOrdersQuery, List<OrderListResponse>>
{
    public async Task<Result<List<OrderListResponse>>> Handle(GetOrdersQuery query, CancellationToken cancellationToken)
    {
        IQueryable<Domain.Orders.Order> ordersQuery = context.Orders.AsNoTracking();

        if (query.CustomerId.HasValue)
        {
            ordersQuery = ordersQuery.Where(o => o.CustomerId == query.CustomerId.Value);
        }

        if (query.Status.HasValue)
        {
            ordersQuery = ordersQuery.Where(o => o.Status == query.Status.Value);
        }

        if (query.FromDate.HasValue)
        {
            ordersQuery = ordersQuery.Where(o => o.OrderDate >= query.FromDate.Value);
        }

        if (query.ToDate.HasValue)
        {
            ordersQuery = ordersQuery.Where(o => o.OrderDate <= query.ToDate.Value);
        }

        List<OrderListResponse> orders = await ordersQuery
            .OrderByDescending(o => o.OrderDate)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(o => new OrderListResponse
            {
                Id = o.Id,
                CustomerId = o.CustomerId,
                CustomerName = o.Customer!.FirstName + " " + o.Customer.LastName,
                OrderDate = o.OrderDate,
                Status = o.Status.ToString(),
                TotalAmount = o.TotalAmount,
                ItemCount = o.Items.Count
            })
            .ToListAsync(cancellationToken);

        return orders;
    }
}
