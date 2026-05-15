using Domain.Products;

namespace Domain.Supplies;

public sealed class SupplyOrderLine
{
    public Guid Id { get; set; }
    public Guid SupplyOrderId { get; set; }
    public SupplyOrder SupplyOrder { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }
}
