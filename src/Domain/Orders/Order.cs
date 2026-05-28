using Domain.Customers;
using SharedKernel;

namespace Domain.Orders;

public sealed class Order : Entity
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public OrderStatus Status { get; set; }
    public DateTime PlacedAt { get; set; }
    public decimal Total { get; set; }
    public List<OrderLine> OrderLines { get; set; } = [];
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
}
