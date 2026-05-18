using Application.Abstractions.Messaging;

namespace Application.Catalog.Suppliers.GetById;

public sealed record GetSupplierByIdQuery(Guid SupplierId) : IQuery<SupplierResponse>;
