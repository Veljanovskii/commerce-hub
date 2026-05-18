using SharedKernel;

namespace Domain.Customers;

public sealed record CustomerRegisteredDomainEvent(Guid CustomerId) : IDomainEvent;
