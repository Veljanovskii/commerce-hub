using Application.Abstractions.Messaging;
using Domain.Orders;

namespace Application.Orders.UpdateStatus;

public sealed class UpdateOrderStatusCommand : ICommand
{
    public Guid OrderId { get; set; }
    public OrderStatus NewStatus { get; set; }
}
