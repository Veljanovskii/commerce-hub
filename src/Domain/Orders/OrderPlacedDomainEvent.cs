using SharedKernel;

namespace Domain.Orders;

public sealed record OrderPlacedDomainEvent(Guid OrderId) : IDomainEvent;
