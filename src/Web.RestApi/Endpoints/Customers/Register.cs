using Application.Abstractions.Messaging;
using Application.Customers;
using Application.Customers.Register;
using SharedKernel;
using Web.RestApi.Extensions;
using Web.RestApi.Infrastructure;

namespace Web.RestApi.Endpoints.Customers;

internal sealed class Register : IEndpoint
{
    public sealed class Request
    {
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/customers/register", async (
            Request request,
            ICommandHandler<RegisterCustomerCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new RegisterCustomerCommand
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Password = request.Password,
                Phone = request.Phone,
                Address = request.Address,
                City = request.City,
                Country = request.Country
            };

            Result<Guid> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Customers);
    }
}
