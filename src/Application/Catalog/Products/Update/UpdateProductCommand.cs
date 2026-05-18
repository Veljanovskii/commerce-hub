using Application.Abstractions.Messaging;

namespace Application.Catalog.Products.Update;

public sealed class UpdateProductCommand : ICommand
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public Guid CategoryId { get; set; }
    public Guid SupplierId { get; set; }
}
