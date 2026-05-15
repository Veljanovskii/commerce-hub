using System.Data.Common;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Web.Shared.Diagnostics;

namespace Web.Shared.Interceptors;

public sealed class BackendCallCountingInterceptor : DbCommandInterceptor
{
    private static readonly KeyValuePair<string, object?>[] Tags =
        [new("db.system", "postgresql")];

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        CommerceHubDiagnostics.BackendCalls.Add(1, Tags);
        return base.ReaderExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        CommerceHubDiagnostics.BackendCalls.Add(1, Tags);
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> NonQueryExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result)
    {
        CommerceHubDiagnostics.BackendCalls.Add(1, Tags);
        return base.NonQueryExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        CommerceHubDiagnostics.BackendCalls.Add(1, Tags);
        return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override InterceptionResult<object> ScalarExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result)
    {
        CommerceHubDiagnostics.BackendCalls.Add(1, Tags);
        return base.ScalarExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
    {
        CommerceHubDiagnostics.BackendCalls.Add(1, Tags);
        return base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
    }
}
