using Application.Abstractions.Messaging;
using Application.Catalog.Suppliers;
using Application.Catalog.Suppliers.Create;
using SharedKernel;
using Web.RestApi.Extensions;
using Web.RestApi.Infrastructure;

namespace Web.RestApi.Endpoints.Suppliers;

internal sealed class Create : IEndpoint
{
    public sealed class Request
    {
        public string CompanyName { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/suppliers", async (
            Request request,
            ICommandHandler<CreateSupplierCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateSupplierCommand
            {
                CompanyName = request.CompanyName,
                ContactName = request.ContactName,
                Email = request.Email,
                Phone = request.Phone,
                Address = request.Address,
                City = request.City,
                Country = request.Country
            };

            Result<Guid> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Suppliers)
        .RequireAuthorization();
    }
}
