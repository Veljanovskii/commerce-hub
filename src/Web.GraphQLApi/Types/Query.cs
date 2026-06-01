using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Orders.GetHistory;
using Application.Products.GetHistory;
using Domain.Customers;
using Domain.Orders;
using Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Web.GraphQLApi.Types;

public class Query
{
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Product> GetProducts([Service] IApplicationDbContext dbContext) =>
        dbContext.Products.AsNoTracking();

    [UseFiltering]
    [UseSorting]
    public IQueryable<Product> GetProductsWithDetails(
    [Service] IApplicationDbContext dbContext,
    int page = 1,
    int pageSize = 50)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return dbContext.Products
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
    }

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Order> GetOrders([Service] IApplicationDbContext dbContext) =>
        dbContext.Orders.AsNoTracking();

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Customer> GetCustomers([Service] IApplicationDbContext dbContext) =>
        dbContext.Customers.AsNoTracking();

    public async Task<Product?> GetProductById(
        Guid id,
        [Service] IApplicationDbContext dbContext,
        CancellationToken cancellationToken) =>
        await dbContext.Products
            .AsNoTracking()
            .Include(p => p.Category)
                .ThenInclude(c => c.ParentCategory)
            .Include(p => p.StockItems)
                .ThenInclude(s => s.Supplier)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<Order?> GetOrderById(
        Guid id,
        [Service] IApplicationDbContext dbContext,
        CancellationToken cancellationToken) =>
        await dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
                .ThenInclude(c => c.Addresses)
            .Include(o => o.OrderLines)
                .ThenInclude(l => l.Product)
                    .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<string?> GetOrderStatus(
        Guid id,
        [Service] IApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        Order? order = await dbContext.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        return order?.Status.ToString();
    }

    public async Task<List<OrderHistoryEntry>?> GetOrderHistory(
        Guid id,
        [Service] IQueryHandler<GetOrderHistoryQuery, List<OrderHistoryEntry>> handler,
        CancellationToken cancellationToken)
    {
        SharedKernel.Result<List<OrderHistoryEntry>> result = await handler.Handle(
            new GetOrderHistoryQuery(id), cancellationToken);

        return result.IsSuccess ? result.Value : null;
    }

    public async Task<List<ProductHistoryEntry>?> GetProductHistory(
        Guid id,
        [Service] IQueryHandler<GetProductHistoryQuery, List<ProductHistoryEntry>> handler,
        CancellationToken cancellationToken)
    {
        SharedKernel.Result<List<ProductHistoryEntry>> result = await handler.Handle(
            new GetProductHistoryQuery(id), cancellationToken);

        return result.IsSuccess ? result.Value : null;
    }
}
