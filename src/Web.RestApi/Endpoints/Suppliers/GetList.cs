using Application.Abstractions.Messaging;
using Application.Catalog.Suppliers;
using Application.Catalog.Suppliers.GetList;
using SharedKernel;
using Web.RestApi.Extensions;
using Web.RestApi.Infrastructure;

namespace Web.RestApi.Endpoints.Suppliers;

internal sealed class GetList : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/suppliers", async (
            IQueryHandler<GetSuppliersQuery, List<SupplierResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetSuppliersQuery();

            Result<List<SupplierResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Suppliers)
        .RequireAuthorization();
    }
}
