using Application.Abstractions.Messaging;
using Application.Catalog.Categories;
using Application.Catalog.Categories.GetById;
using Application.Catalog.Categories.GetList;
using HotChocolate.Authorization;
using SharedKernel;

namespace Web.GraphQLApi.Queries;

[ExtendObjectType(OperationTypeNames.Query)]
public sealed class CategoryQueries
{
    [Authorize]
    public async Task<CategoryResponse> GetCategoryById(
        Guid categoryId,
        [Service] IQueryHandler<GetCategoryByIdQuery, CategoryResponse> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetCategoryByIdQuery(categoryId);
        SharedKernel.Result<CategoryResponse> result = await handler.Handle(query, cancellationToken);
        return result.Value;
    }

    [Authorize]
    public async Task<List<CategoryResponse>> GetCategories(
        [Service] IQueryHandler<GetCategoriesQuery, List<CategoryResponse>> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetCategoriesQuery();
        SharedKernel.Result<List<CategoryResponse>> result = await handler.Handle(query, cancellationToken);
        return result.Value;
    }
}
