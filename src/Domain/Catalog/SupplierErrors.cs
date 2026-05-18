using SharedKernel;

namespace Domain.Catalog;

public static class SupplierErrors
{
    public static Error NotFound(Guid supplierId) => Error.NotFound(
        "Suppliers.NotFound",
        $"The supplier with the Id = '{supplierId}' was not found");

    public static Error EmailAlreadyExists(string email) => Error.Conflict(
        "Suppliers.EmailAlreadyExists",
        $"A supplier with the email = '{email}' already exists");
}
