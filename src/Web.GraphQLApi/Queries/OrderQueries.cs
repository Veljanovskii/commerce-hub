using Application.Abstractions.Messaging;
using Application.Orders;
using Application.Orders.GetById;
using Application.Orders.GetList;
using Domain.Orders;
using HotChocolate.Authorization;
using SharedKernel;

namespace Web.GraphQLApi.Queries;

[ExtendObjectType(OperationTypeNames.Query)]
public sealed class OrderQueries
{
    [Authorize]
    public async Task<OrderDetailResponse> GetOrderById(
        Guid orderId,
        [Service] IQueryHandler<GetOrderByIdQuery, OrderDetailResponse> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetOrderByIdQuery(orderId);
        SharedKernel.Result<OrderDetailResponse> result = await handler.Handle(query, cancellationToken);
        return result.Value;
    }

    [Authorize]
    public async Task<List<OrderListResponse>> GetOrders(
        Guid? customerId,
        OrderStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        int? page,
        int? pageSize,
        [Service] IQueryHandler<GetOrdersQuery, List<OrderListResponse>> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetOrdersQuery
        {
            CustomerId = customerId,
            Status = status,
            FromDate = fromDate,
            ToDate = toDate,
            Page = page ?? 1,
            PageSize = pageSize ?? 20
        };

        SharedKernel.Result<List<OrderListResponse>> result = await handler.Handle(query, cancellationToken);
        return result.Value;
    }
}
