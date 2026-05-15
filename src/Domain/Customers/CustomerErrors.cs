using SharedKernel;

namespace Domain.Customers;

public static class CustomerErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        "Customers.NotFound", $"Customer with id '{id}' was not found.");
}
