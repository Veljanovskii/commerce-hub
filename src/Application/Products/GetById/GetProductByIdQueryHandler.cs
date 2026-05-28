using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.GetById;

internal sealed class GetProductByIdQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetProductByIdQuery, ProductResponse>
{
    public async Task<Result<ProductResponse>> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        ProductResponse? product = await dbContext.Products
            .AsNoTracking()
            .Where(p => p.Id == query.ProductId)
            .Select(p => new ProductResponse(
                p.Id,
                p.Name,
                p.Sku,
                p.Description,
                p.Price,
                p.CategoryId,
                p.Category.Name))
            .FirstOrDefaultAsync(cancellationToken);

        if (product is null)
        {
            return Result.Failure<ProductResponse>(ProductErrors.NotFound(query.ProductId));
        }

        return product;
    }
}
