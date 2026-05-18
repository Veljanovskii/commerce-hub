using Application.Abstractions.Messaging;
using Domain.Orders;

namespace Application.Orders.GetList;

public sealed class GetOrdersQuery : IQuery<List<OrderListResponse>>
{
    public Guid? CustomerId { get; set; }
    public OrderStatus? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
