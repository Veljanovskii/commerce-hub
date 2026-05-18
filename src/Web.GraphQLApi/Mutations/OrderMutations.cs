using Application.Abstractions.Messaging;
using Application.Orders.Create;
using Application.Orders.UpdateStatus;
using HotChocolate.Authorization;
using SharedKernel;

namespace Web.GraphQLApi.Mutations;

[ExtendObjectType(OperationTypeNames.Mutation)]
public sealed class OrderMutations
{
    [Authorize]
    public async Task<Guid> CreateOrder(
        CreateOrderCommand input,
        [Service] ICommandHandler<CreateOrderCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        SharedKernel.Result<Guid> result = await handler.Handle(input, cancellationToken);
        return result.Value;
    }

    [Authorize]
    public async Task<bool> UpdateOrderStatus(
        UpdateOrderStatusCommand input,
        [Service] ICommandHandler<UpdateOrderStatusCommand> handler,
        CancellationToken cancellationToken)
    {
        SharedKernel.Result result = await handler.Handle(input, cancellationToken);
        return result.IsSuccess;
    }
}
