using Application.Abstractions.Messaging;

namespace Application.Orders.Create;

public sealed class CreateOrderCommand : ICommand<Guid>
{
    public Guid CustomerId { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string ShippingCity { get; set; } = string.Empty;
    public string ShippingCountry { get; set; } = string.Empty;
    public List<CreateOrderItemDto> Items { get; set; } = [];
}

public sealed class CreateOrderItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Discount { get; set; }
}
