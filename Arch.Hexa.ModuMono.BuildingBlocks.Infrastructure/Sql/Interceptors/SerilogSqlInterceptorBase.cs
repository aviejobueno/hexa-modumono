using System.Collections.Concurrent;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Serilog;
using Serilog.Context;

namespace Arch.Hexa.ModuMono.BuildingBlocks.Infrastructure.Sql.Interceptors;

public abstract class SerilogSqlInterceptorBase(string dbRole) : DbCommandInterceptor
{
    private static readonly ConcurrentDictionary<Guid, System.Diagnostics.Stopwatch> Timers = new();

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        // Start timer per command id (thread-safe for pooled contexts)
        Timers[eventData.CommandId] = System.Diagnostics.Stopwatch.StartNew();
        return result;
    }

    public override DbDataReader ReaderExecuted(
        DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
    {
        LogSuccess(command, eventData);
        return result;
    }

    public override void CommandFailed(DbCommand command, CommandErrorEventData eventData)
    {
        LogFailure(command, eventData);
    }

    private void LogSuccess(DbCommand command, CommandExecutedEventData eventData)
    {
        Timers.TryRemove(eventData.CommandId, out var sw);
        sw?.Stop();

        var paramNames = command.Parameters
            .Cast<DbParameter>()
            .Select(p => p.ParameterName)
            .ToArray();

        using (LogContext.PushProperty("DbRole", dbRole))
        using (LogContext.PushProperty("DbCommandId", eventData.CommandId))
        using (LogContext.PushProperty("DbConnectionId", eventData.ConnectionId))
        using (LogContext.PushProperty("DbDurationMs", sw?.Elapsed.TotalMilliseconds))
        using (LogContext.PushProperty("DbParamNames", paramNames))
        {
            Log.Information("SQL executed: {Sql}", command.CommandText);
        }
    }


    private void LogFailure(DbCommand command, CommandErrorEventData eventData)
    {
        var paramNames = command.Parameters
            .Cast<DbParameter>()
            .Select(p => p.ParameterName)
            .ToArray();

        using (LogContext.PushProperty("DbRole", dbRole))
        using (LogContext.PushProperty("DbCommandId", eventData.CommandId))
        using (LogContext.PushProperty("DbConnectionId", eventData.ConnectionId))
        using (LogContext.PushProperty("DbParamNames", paramNames))
        {
            Log.Error(eventData.Exception, "SQL failed: {Sql}", command.CommandText);
        }
    }
}
