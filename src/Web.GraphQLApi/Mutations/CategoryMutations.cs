using Application.Abstractions.Messaging;
using Application.Catalog.Categories.Create;
using HotChocolate.Authorization;
using SharedKernel;

namespace Web.GraphQLApi.Mutations;

[ExtendObjectType(OperationTypeNames.Mutation)]
public sealed class CategoryMutations
{
    [Authorize]
    public async Task<Guid> CreateCategory(
        CreateCategoryCommand input,
        [Service] ICommandHandler<CreateCategoryCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        SharedKernel.Result<Guid> result = await handler.Handle(input, cancellationToken);
        return result.Value;
    }
}
