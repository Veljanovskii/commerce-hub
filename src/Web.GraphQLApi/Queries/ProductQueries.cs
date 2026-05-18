using Application.Abstractions.Messaging;
using Application.Catalog.Products;
using Application.Catalog.Products.GetById;
using Application.Catalog.Products.GetList;
using HotChocolate.Authorization;
using SharedKernel;

namespace Web.GraphQLApi.Queries;

[ExtendObjectType(OperationTypeNames.Query)]
public sealed class ProductQueries
{
    [Authorize]
    public async Task<ProductDetailResponse> GetProductById(
        Guid productId,
        [Service] IQueryHandler<GetProductByIdQuery, ProductDetailResponse> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetProductByIdQuery(productId);
        SharedKernel.Result<ProductDetailResponse> result = await handler.Handle(query, cancellationToken);
        return result.Value;
    }

    [Authorize]
    public async Task<List<ProductListResponse>> GetProducts(
        Guid? categoryId,
        Guid? supplierId,
        decimal? minPrice,
        decimal? maxPrice,
        string? searchTerm,
        int? page,
        int? pageSize,
        [Service] IQueryHandler<GetProductsQuery, List<ProductListResponse>> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetProductsQuery
        {
            CategoryId = categoryId,
            SupplierId = supplierId,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            SearchTerm = searchTerm,
            Page = page ?? 1,
            PageSize = pageSize ?? 20
        };

        SharedKernel.Result<List<ProductListResponse>> result = await handler.Handle(query, cancellationToken);
        return result.Value;
    }
}
