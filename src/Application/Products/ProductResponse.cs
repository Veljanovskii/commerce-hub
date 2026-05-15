namespace Application.Products;

public sealed record ProductResponse(
    Guid Id,
    string Name,
    string Sku,
    string Description,
    decimal Price,
    Guid CategoryId,
    string CategoryName);
