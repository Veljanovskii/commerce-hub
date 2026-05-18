using Application.Abstractions.Messaging;

namespace Application.Orders.Dashboard;

public sealed record GetDashboardQuery() : IQuery<DashboardResponse>;
