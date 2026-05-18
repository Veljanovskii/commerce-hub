using SharedKernel;

namespace Domain.Catalog;

public static class ProductErrors
{
    public static Error NotFound(Guid productId) => Error.NotFound(
        "Products.NotFound",
        $"The product with the Id = '{productId}' was not found");

    public static Error SkuAlreadyExists(string sku) => Error.Conflict(
        "Products.SkuAlreadyExists",
        $"A product with the SKU = '{sku}' already exists");

    public static Error InsufficientStock(Guid productId, int requested, int available) => Error.Problem(
        "Products.InsufficientStock",
        $"Product '{productId}' has insufficient stock. Requested: {requested}, Available: {available}");

    public static Error InvalidPrice() => Error.Problem(
        "Products.InvalidPrice",
        "Product price must be greater than zero");
}
