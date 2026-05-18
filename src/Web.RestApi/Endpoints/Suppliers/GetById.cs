using Application.Abstractions.Messaging;
using Application.Catalog.Suppliers;
using Application.Catalog.Suppliers.GetById;
using SharedKernel;
using Web.RestApi.Extensions;
using Web.RestApi.Infrastructure;

namespace Web.RestApi.Endpoints.Suppliers;

internal sealed class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/suppliers/{supplierId:guid}", async (
            Guid supplierId,
            IQueryHandler<GetSupplierByIdQuery, SupplierResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetSupplierByIdQuery(supplierId);

            Result<SupplierResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Suppliers)
        .RequireAuthorization();
    }
}
