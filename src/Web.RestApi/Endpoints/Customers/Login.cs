using Application.Abstractions.Messaging;
using Application.Customers.Login;
using SharedKernel;
using Web.RestApi.Extensions;
using Web.RestApi.Infrastructure;

namespace Web.RestApi.Endpoints.Customers;

internal sealed class Login : IEndpoint
{
    public sealed class Request
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/customers/login", async (
            Request request,
            ICommandHandler<LoginCustomerCommand, string> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new LoginCustomerCommand
            {
                Email = request.Email,
                Password = request.Password
            };

            Result<string> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Customers);
    }
}
