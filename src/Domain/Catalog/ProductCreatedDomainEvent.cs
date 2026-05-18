using SharedKernel;

namespace Domain.Catalog;

public sealed record ProductCreatedDomainEvent(Guid ProductId) : IDomainEvent;
