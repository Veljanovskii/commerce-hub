using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.GetList;

internal sealed class GetProductListQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetProductListQuery, List<ProductResponse>>
{
    public async Task<Result<List<ProductResponse>>> Handle(
        GetProductListQuery query,
        CancellationToken cancellationToken)
    {
        IQueryable<Domain.Products.Product> productsQuery = dbContext.Products.AsNoTracking();

        if (query.CategoryId.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.CategoryId == query.CategoryId.Value);
        }

        List<ProductResponse> items = await productsQuery
            .OrderBy(p => p.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p => new ProductResponse(
                p.Id,
                p.Name,
                p.Sku,
                p.Description,
                p.Price,
                p.CategoryId,
                p.Category.Name))
            .ToListAsync(cancellationToken);

        return items;
    }
}
