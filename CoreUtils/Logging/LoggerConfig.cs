using Serilog;
using Serilog.Events;
using Serilog.Configuration;
using Serilog.Core;

namespace HardDev.CoreUtils.Logging;

/// <summary>
/// AppLogger Configuration class for setting up the logger properties.
/// </summary>
public class LoggerConfig
{
    /// <summary>
    /// Gets or sets the logger context name.
    /// </summary>
    public string LoggerContextName { get; set; } = "Main";

    /// <summary>
    /// Gets or sets the log directory path.
    /// </summary>
    public string LogDirectory { get; set; } = "Logs";

    /// <summary>
    /// Gets or sets the log file name.
    /// </summary>
    public string LogFileName { get; set; } = "log_.txt";

    /// <summary>
    /// Gets or sets the output template for the log messages.
    /// </summary>
    public string OutputTemplate { get; set; } =
        "{Timestamp:HH:mm:ss}|{Level:u1}|{Context}|{categoryName} {Message:lj}{NewLine}{Exception}";

    /// <summary>
    /// Gets or sets a value indicating whether console logging is enabled.
    /// </summary>
    public bool EnableConsoleLogging { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether debug logging is enabled.
    /// </summary>
    public bool EnableDebugLogging { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether file logging is enabled.
    /// </summary>
    public bool EnableFileLogging { get; set; } = true;

    /// <summary>
    /// Gets or sets the rolling interval for log files.
    /// </summary>
    public RollingInterval RollingInterval { get; set; } = RollingInterval.Day;

    /// <summary>
    /// Gets or sets the minimum log event level for console logging.
    /// </summary>
    public LogEventLevel ConsoleLogLevel { get; set; } = LogEventLevel.Verbose;

    /// <summary>
    /// Gets or sets the minimum log event level for debug logging.
    /// </summary>
    public LogEventLevel DebugLogLevel { get; set; } = LogEventLevel.Verbose;
    
    /// <summary>
    /// Gets or sets the minimum log event level for file logging.
    /// </summary>
    public LogEventLevel FileLogLevel { get; set; } = LogEventLevel.Verbose;

    /// <summary>
    /// Gets or sets the additional sinks for logger configuration.
    /// </summary>
    public IEnumerable<ILogEventSink> AdditionalSinks { get; set; } = new List<ILogEventSink>();
}