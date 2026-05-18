using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.Dashboard;

internal sealed class GetDashboardQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetDashboardQuery, DashboardResponse>
{
    public async Task<Result<DashboardResponse>> Handle(GetDashboardQuery query, CancellationToken cancellationToken)
    {
        List<TopProductResponse> topProducts = await context.OrderItems
            .AsNoTracking()
            .GroupBy(oi => new { oi.ProductId, oi.Product!.Name })
            .Select(g => new TopProductResponse
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.Name,
                TotalQuantitySold = g.Sum(oi => oi.Quantity),
                TotalRevenue = g.Sum(oi => oi.UnitPrice * oi.Quantity - oi.Discount)
            })
            .OrderByDescending(p => p.TotalQuantitySold)
            .Take(10)
            .ToListAsync(cancellationToken);

        List<RecentOrderResponse> recentOrders = await context.Orders
            .AsNoTracking()
            .OrderByDescending(o => o.OrderDate)
            .Take(10)
            .Select(o => new RecentOrderResponse
            {
                OrderId = o.Id,
                CustomerName = o.Customer!.FirstName + " " + o.Customer.LastName,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        List<LowStockProductResponse> lowStockProducts = await context.Products
            .AsNoTracking()
            .Where(p => p.StockQuantity < 10)
            .OrderBy(p => p.StockQuantity)
            .Take(10)
            .Select(p => new LowStockProductResponse
            {
                ProductId = p.Id,
                ProductName = p.Name,
                Sku = p.Sku,
                StockQuantity = p.StockQuantity
            })
            .ToListAsync(cancellationToken);

        int totalOrders = await context.Orders.CountAsync(cancellationToken);
        int totalProducts = await context.Products.CountAsync(cancellationToken);
        int totalCustomers = await context.Customers.CountAsync(cancellationToken);
        decimal totalRevenue = await context.Orders.SumAsync(o => o.TotalAmount, cancellationToken);

        return new DashboardResponse
        {
            TopProducts = topProducts,
            RecentOrders = recentOrders,
            LowStockProducts = lowStockProducts,
            TotalOrders = totalOrders,
            TotalProducts = totalProducts,
            TotalCustomers = totalCustomers,
            TotalRevenue = totalRevenue
        };
    }
}
