using Application.Abstractions.Messaging;
using Application.Orders.Dashboard;
using SharedKernel;
using Web.RestApi.Extensions;
using Web.RestApi.Infrastructure;

namespace Web.RestApi.Endpoints.Dashboard;

internal sealed class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/dashboard", async (
            IQueryHandler<GetDashboardQuery, DashboardResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetDashboardQuery();
            Result<DashboardResponse> result = await handler.Handle(query, cancellationToken);
            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Dashboard)
        .RequireAuthorization();
    }
}
