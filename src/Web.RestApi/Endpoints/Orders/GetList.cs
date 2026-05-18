using Application.Abstractions.Messaging;
using Application.Orders;
using Application.Orders.GetList;
using Domain.Orders;
using SharedKernel;
using Web.RestApi.Extensions;
using Web.RestApi.Infrastructure;

namespace Web.RestApi.Endpoints.Orders;

internal sealed class GetList : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/orders", async (
            Guid? customerId,
            OrderStatus? status,
            DateTime? fromDate,
            DateTime? toDate,
            int? page,
            int? pageSize,
            IQueryHandler<GetOrdersQuery, List<OrderListResponse>> handler,
            CancellationToken cancellationToken) =>
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

            Result<List<OrderListResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Orders)
        .RequireAuthorization();
    }
}
