namespace Application.Orders;

public sealed record OrderDetailResponse(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    string CustomerEmail,
    string Status,
    DateTime PlacedAt,
    decimal Total,
    List<OrderLineDto> OrderLines);

public sealed record OrderLineDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductSku,
    int Quantity,
    decimal UnitPriceAtOrder);
