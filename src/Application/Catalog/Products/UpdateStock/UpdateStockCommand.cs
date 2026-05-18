using Application.Abstractions.Messaging;

namespace Application.Catalog.Products.UpdateStock;

public sealed class UpdateStockCommand : ICommand
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
