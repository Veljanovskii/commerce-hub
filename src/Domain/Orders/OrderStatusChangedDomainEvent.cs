using SharedKernel;

namespace Domain.Orders;

public sealed record OrderStatusChangedDomainEvent(Guid OrderId, OrderStatus OldStatus, OrderStatus NewStatus) : IDomainEvent;
