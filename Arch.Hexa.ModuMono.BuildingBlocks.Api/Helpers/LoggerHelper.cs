using Serilog.Events;

namespace Arch.Hexa.ModuMono.BuildingBlocks.Api.Helpers;

public static class LoggerHelper
{
    public static LogEventLevel GetLoggingLevel(int level)
    {
        var levelToReturn = level switch
        {
            0 => LogEventLevel.Verbose,
            1 => LogEventLevel.Debug,
            2 => LogEventLevel.Information,
            3 => LogEventLevel.Warning,
            4 => LogEventLevel.Error,
            5 => LogEventLevel.Fatal,
            _ => LogEventLevel.Information
        };

        return levelToReturn;
    }
}