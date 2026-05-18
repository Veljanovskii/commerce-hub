using SharedKernel;

namespace Domain.Catalog;

public sealed record StockUpdatedDomainEvent(Guid ProductId, int NewQuantity) : IDomainEvent;
