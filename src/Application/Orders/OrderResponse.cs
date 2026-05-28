namespace Application.Orders;

public sealed record OrderResponse(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    string Status,
    DateTime PlacedAt,
    decimal Total);
