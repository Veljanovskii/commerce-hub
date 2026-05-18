using Application.Abstractions.Messaging;
using Application.Catalog.Suppliers.Create;
using HotChocolate.Authorization;
using SharedKernel;

namespace Web.GraphQLApi.Mutations;

[ExtendObjectType(OperationTypeNames.Mutation)]
public sealed class SupplierMutations
{
    [Authorize]
    public async Task<Guid> CreateSupplier(
        CreateSupplierCommand input,
        [Service] ICommandHandler<CreateSupplierCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        SharedKernel.Result<Guid> result = await handler.Handle(input, cancellationToken);
        return result.Value;
    }
}
