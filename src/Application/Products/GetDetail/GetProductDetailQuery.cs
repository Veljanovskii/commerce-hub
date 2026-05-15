using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.GetDetail;

public sealed record GetProductDetailQuery(Guid ProductId) : IQuery<ProductDetailResponse>;

internal sealed class GetProductDetailQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetProductDetailQuery, ProductDetailResponse>
{
    public async Task<Result<ProductDetailResponse>> Handle(GetProductDetailQuery query, CancellationToken cancellationToken)
    {
        Product? product = await dbContext.Products
            .AsNoTracking()
            .Include(p => p.Category)
                .ThenInclude(c => c.ParentCategory)
            .Include(p => p.StockItems)
                .ThenInclude(s => s.Supplier)
            .FirstOrDefaultAsync(p => p.Id == query.ProductId, cancellationToken);

        if (product is null)
        {
            return Result.Failure<ProductDetailResponse>(ProductErrors.NotFound(query.ProductId));
        }

        return new ProductDetailResponse(
            product.Id,
            product.Name,
            product.Sku,
            product.Description,
            product.Price,
            MapCategory(product.Category),
            product.StockItems.Select(s => new StockItemDto(
                s.Id,
                s.SupplierId,
                s.Supplier.Name,
                s.QuantityOnHand,
                s.ReorderLevel)).ToList());
    }

    private static CategoryDto MapCategory(Category category) =>
        new(category.Id, category.Name,
            category.ParentCategory != null
                ? MapCategory(category.ParentCategory)
                : null);
}
