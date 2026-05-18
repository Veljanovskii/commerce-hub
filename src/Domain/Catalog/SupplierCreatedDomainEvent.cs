using SharedKernel;

namespace Domain.Catalog;

public sealed record SupplierCreatedDomainEvent(Guid SupplierId) : IDomainEvent;
