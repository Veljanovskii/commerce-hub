using Application.Abstractions.Messaging;
using Application.Customers;
using Application.Customers.GetById;
using SharedKernel;
using Web.RestApi.Extensions;
using Web.RestApi.Infrastructure;

namespace Web.RestApi.Endpoints.Customers;

internal sealed class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/customers/{customerId:guid}", async (
            Guid customerId,
            IQueryHandler<GetCustomerByIdQuery, CustomerResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetCustomerByIdQuery(customerId);
            Result<CustomerResponse> result = await handler.Handle(query, cancellationToken);
            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Customers)
        .RequireAuthorization();
    }
}
