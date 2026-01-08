namespace Arch.Hexa.ModuMono.BuildingBlocks.Application.Settings;

public interface ISettings
{
    public string ServiceName { get; }
    public string TelemetryServerUrl { get; }
    public string SeqServerUrl { get; }
    public string DatabaseConnectionString { get; }
    public string CorsAllowedOrigins { get; }
    public bool CustomLoggingConsoleEnabled { get; }
    public int LoggingConsoleLevel { get; }
    public bool CustomLoggingSeqEnabled { get; }
    public int LoggingSeqLevel { get; }
    public bool CustomLoggingFileEnabled { get; }
    public int LoggingFileLevel { get; }
    public string CustomLoggingFilePathFormat { get; }
}