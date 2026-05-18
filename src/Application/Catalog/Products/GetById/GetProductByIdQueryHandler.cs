using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Catalog.Products.GetById;

internal sealed class GetProductByIdQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetProductByIdQuery, ProductDetailResponse>
{
    public async Task<Result<ProductDetailResponse>> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        ProductDetailResponse? product = await context.Products
            .AsNoTracking()
            .Where(p => p.Id == query.ProductId)
            .Select(p => new ProductDetailResponse
            {
                Id = p.Id,
                Name = p.Name,
                Sku = p.Sku,
                Description = p.Description,
                UnitPrice = p.UnitPrice,
                StockQuantity = p.StockQuantity,
                CategoryId = p.CategoryId,
                CategoryName = p.Category!.Name,
                SupplierId = p.SupplierId,
                SupplierCompanyName = p.Supplier!.CompanyName,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (product is null)
        {
            return Result.Failure<ProductDetailResponse>(ProductErrors.NotFound(query.ProductId));
        }

        return product;
    }
}
