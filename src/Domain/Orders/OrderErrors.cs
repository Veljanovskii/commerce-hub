using SharedKernel;

namespace Domain.Orders;

public static class OrderErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        "Orders.NotFound", $"Order with id '{id}' was not found.");
}
