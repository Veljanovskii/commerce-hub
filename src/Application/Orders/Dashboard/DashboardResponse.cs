namespace Application.Orders.Dashboard;

public sealed class DashboardResponse
{
    public List<TopProductResponse> TopProducts { get; set; } = [];
    public List<RecentOrderResponse> RecentOrders { get; set; } = [];
    public List<LowStockProductResponse> LowStockProducts { get; set; } = [];
    public int TotalOrders { get; set; }
    public int TotalProducts { get; set; }
    public int TotalCustomers { get; set; }
    public decimal TotalRevenue { get; set; }
}

public sealed class TopProductResponse
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int TotalQuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
}

public sealed class RecentOrderResponse
{
    public Guid OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}

public sealed class LowStockProductResponse
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
}
