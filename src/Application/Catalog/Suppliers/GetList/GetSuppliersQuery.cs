using Application.Abstractions.Messaging;

namespace Application.Catalog.Suppliers.GetList;

public sealed record GetSuppliersQuery() : IQuery<List<SupplierResponse>>;
