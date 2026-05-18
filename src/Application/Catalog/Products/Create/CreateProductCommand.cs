using Application.Abstractions.Messaging;

namespace Application.Catalog.Products.Create;

public sealed class CreateProductCommand : ICommand<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
    public Guid CategoryId { get; set; }
    public Guid SupplierId { get; set; }
}
