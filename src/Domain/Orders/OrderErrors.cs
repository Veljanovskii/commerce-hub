using SharedKernel;

namespace Domain.Orders;

public static class OrderErrors
{
    public static Error NotFound(Guid orderId) => Error.NotFound(
        "Orders.NotFound",
        $"The order with the Id = '{orderId}' was not found");

    public static Error InvalidStatusTransition(OrderStatus current, OrderStatus target) => Error.Problem(
        "Orders.InvalidStatusTransition",
        $"Cannot transition order from '{current}' to '{target}'");

    public static Error EmptyOrder() => Error.Problem(
        "Orders.EmptyOrder",
        "An order must contain at least one item");

    public static Error AlreadyCancelled(Guid orderId) => Error.Problem(
        "Orders.AlreadyCancelled",
        $"The order with the Id = '{orderId}' has already been cancelled");
}
