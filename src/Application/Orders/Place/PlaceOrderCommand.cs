using Application.Abstractions.Messaging;

namespace Application.Orders.Place;

public sealed record PlaceOrderCommand(
    Guid CustomerId,
    List<PlaceOrderLineItem> Lines) : ICommand<Guid>;

public sealed record PlaceOrderLineItem(Guid ProductId, int Quantity);
