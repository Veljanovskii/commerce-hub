using SharedKernel;

namespace Domain.Supplies;

public sealed class SupplyOrder : Entity
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;
    public SupplyOrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public List<SupplyOrderLine> Lines { get; set; } = [];
}
