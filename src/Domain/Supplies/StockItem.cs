using Domain.Products;
using SharedKernel;

namespace Domain.Supplies;

public sealed class StockItem : Entity
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public Guid SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;
    public int QuantityOnHand { get; set; }
    public int ReorderLevel { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
}
