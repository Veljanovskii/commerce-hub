using Application.Abstractions.Messaging;
using Application.Customers.Login;
using Application.Customers.Register;
using SharedKernel;

namespace Web.GraphQLApi.Mutations;

[ExtendObjectType(OperationTypeNames.Mutation)]
public sealed class CustomerMutations
{
    public async Task<Guid> RegisterCustomer(
        RegisterCustomerCommand input,
        [Service] ICommandHandler<RegisterCustomerCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        SharedKernel.Result<Guid> result = await handler.Handle(input, cancellationToken);
        return result.Value;
    }

    public async Task<string> LoginCustomer(
        LoginCustomerCommand input,
        [Service] ICommandHandler<LoginCustomerCommand, string> handler,
        CancellationToken cancellationToken)
    {
        SharedKernel.Result<string> result = await handler.Handle(input, cancellationToken);
        return result.Value;
    }
}
