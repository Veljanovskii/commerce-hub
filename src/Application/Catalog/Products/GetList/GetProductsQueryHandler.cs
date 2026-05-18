using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Catalog.Products.GetList;

internal sealed class GetProductsQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetProductsQuery, List<ProductListResponse>>
{
    public async Task<Result<List<ProductListResponse>>> Handle(GetProductsQuery query, CancellationToken cancellationToken)
    {
        IQueryable<Domain.Catalog.Product> productsQuery = context.Products.AsNoTracking();

        if (query.CategoryId.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.CategoryId == query.CategoryId.Value);
        }

        if (query.SupplierId.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.SupplierId == query.SupplierId.Value);
        }

        if (query.MinPrice.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.UnitPrice >= query.MinPrice.Value);
        }

        if (query.MaxPrice.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.UnitPrice <= query.MaxPrice.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            productsQuery = productsQuery.Where(p =>
                p.Name.Contains(query.SearchTerm) || p.Description.Contains(query.SearchTerm));
        }

        List<ProductListResponse> products = await productsQuery
            .OrderBy(p => p.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p => new ProductListResponse
            {
                Id = p.Id,
                Name = p.Name,
                Sku = p.Sku,
                UnitPrice = p.UnitPrice,
                StockQuantity = p.StockQuantity,
                CategoryName = p.Category!.Name
            })
            .ToListAsync(cancellationToken);

        return products;
    }
}
