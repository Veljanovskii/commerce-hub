using SharedKernel;

namespace Domain.Supplies;

public sealed class Supplier : Entity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public List<StockItem> StockItems { get; set; } = [];
    public List<SupplyOrder> SupplyOrders { get; set; } = [];
}
