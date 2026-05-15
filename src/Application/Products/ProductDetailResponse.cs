namespace Application.Products;

public sealed record ProductDetailResponse(
    Guid Id,
    string Name,
    string Sku,
    string Description,
    decimal Price,
    CategoryDto Category,
    List<StockItemDto> StockItems);

public sealed record CategoryDto(
    Guid Id,
    string Name,
    CategoryDto? ParentCategory);

public sealed record StockItemDto(
    Guid Id,
    Guid SupplierId,
    string SupplierName,
    int QuantityOnHand,
    int ReorderLevel);
