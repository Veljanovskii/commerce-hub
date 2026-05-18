using Application.Abstractions.Messaging;
using Application.Customers;
using Application.Customers.GetById;
using HotChocolate.Authorization;
using SharedKernel;

namespace Web.GraphQLApi.Queries;

[ExtendObjectType(OperationTypeNames.Query)]
public sealed class CustomerQueries
{
    [Authorize]
    public async Task<CustomerResponse> GetCustomerById(
        Guid customerId,
        [Service] IQueryHandler<GetCustomerByIdQuery, CustomerResponse> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetCustomerByIdQuery(customerId);
        SharedKernel.Result<CustomerResponse> result = await handler.Handle(query, cancellationToken);
        return result.Value;
    }
}
