using Application.Abstractions.Messaging;
using Application.Catalog.Suppliers;
using Application.Catalog.Suppliers.GetById;
using Application.Catalog.Suppliers.GetList;
using HotChocolate.Authorization;
using SharedKernel;

namespace Web.GraphQLApi.Queries;

[ExtendObjectType(OperationTypeNames.Query)]
public sealed class SupplierQueries
{
    [Authorize]
    public async Task<SupplierResponse> GetSupplierById(
        Guid supplierId,
        [Service] IQueryHandler<GetSupplierByIdQuery, SupplierResponse> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetSupplierByIdQuery(supplierId);
        SharedKernel.Result<SupplierResponse> result = await handler.Handle(query, cancellationToken);
        return result.Value;
    }

    [Authorize]
    public async Task<List<SupplierResponse>> GetSuppliers(
        [Service] IQueryHandler<GetSuppliersQuery, List<SupplierResponse>> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetSuppliersQuery();
        SharedKernel.Result<List<SupplierResponse>> result = await handler.Handle(query, cancellationToken);
        return result.Value;
    }
}
