using Application.Abstractions.Messaging;
using Application.Catalog.Products;
using Application.Catalog.Products.GetList;
using SharedKernel;
using Web.RestApi.Extensions;
using Web.RestApi.Infrastructure;

namespace Web.RestApi.Endpoints.Products;

internal sealed class GetList : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/products", async (
            Guid? categoryId,
            Guid? supplierId,
            decimal? minPrice,
            decimal? maxPrice,
            string? searchTerm,
            int? page,
            int? pageSize,
            IQueryHandler<GetProductsQuery, List<ProductListResponse>> handler,
            CancellationToken cancellationToken) =>
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

            Result<List<ProductListResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Products)
        .RequireAuthorization();
    }
}
