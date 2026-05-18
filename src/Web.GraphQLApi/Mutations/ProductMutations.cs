using Application.Abstractions.Messaging;
using Application.Catalog.Products.Create;
using Application.Catalog.Products.Update;
using Application.Catalog.Products.UpdateStock;
using HotChocolate.Authorization;
using SharedKernel;

namespace Web.GraphQLApi.Mutations;

[ExtendObjectType(OperationTypeNames.Mutation)]
public sealed class ProductMutations
{
    [Authorize]
    public async Task<Guid> CreateProduct(
        CreateProductCommand input,
        [Service] ICommandHandler<CreateProductCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        SharedKernel.Result<Guid> result = await handler.Handle(input, cancellationToken);
        return result.Value;
    }

    [Authorize]
    public async Task<bool> UpdateProduct(
        UpdateProductCommand input,
        [Service] ICommandHandler<UpdateProductCommand> handler,
        CancellationToken cancellationToken)
    {
        SharedKernel.Result result = await handler.Handle(input, cancellationToken);
        return result.IsSuccess;
    }

    [Authorize]
    public async Task<bool> UpdateStock(
        UpdateStockCommand input,
        [Service] ICommandHandler<UpdateStockCommand> handler,
        CancellationToken cancellationToken)
    {
        SharedKernel.Result result = await handler.Handle(input, cancellationToken);
        return result.IsSuccess;
    }
}
