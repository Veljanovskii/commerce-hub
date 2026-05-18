using Application.Abstractions.Messaging;
using Application.Orders.Dashboard;
using HotChocolate.Authorization;
using SharedKernel;

namespace Web.GraphQLApi.Queries;

[ExtendObjectType(OperationTypeNames.Query)]
public sealed class DashboardQueries
{
    [Authorize]
    public async Task<DashboardResponse> GetDashboard(
        [Service] IQueryHandler<GetDashboardQuery, DashboardResponse> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetDashboardQuery();
        SharedKernel.Result<DashboardResponse> result = await handler.Handle(query, cancellationToken);
        return result.Value;
    }
}
