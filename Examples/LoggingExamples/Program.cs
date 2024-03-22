using HardDev.CoreUtils.Logging;
using Serilog;
using Serilog.Events;

namespace HardDev.LoggingExamples
{
    public static class Program
    {
        // A default logger configuration, useful for most cases
        private static readonly ILogger _defaultLogger = AppLogger.ForContext(nameof(Program));

        // A custom logger tailored to specific needs
        private static readonly ILogger _customLogger = CreateCustomLogger();

        public static async Task Main()
        {
            // Register global event handlers for logging
            AppLogger.RegisterGlobalEventHandlers();

            // Examples of logging messages at various levels using the default logger
            _defaultLogger.Verbose("This is a verbose log message");
            _defaultLogger.Debug("This is a debug log message");
            _defaultLogger.Information("This is an information log message");
            _defaultLogger.Warning("This is a warning log message");
            _defaultLogger.Error("This is an error log message");
            _defaultLogger.Fatal("This is a fatal log message");

            try
            {
                await SimulateExceptionAsync();
            }
            catch (Exception ex)
            {
                _defaultLogger.Error(ex, "An exception occurred");
            }

            // Examples of logging messages with custom logger
            _customLogger.Information("Custom logger: This is an information log message");
            _customLogger.Warning("Custom logger: This is a warning log message");
        }

        private static ILogger CreateCustomLogger()
        {
            var customConfig = new LoggerConfig
            {
                ContextName = "CustomContext",
                LogPath = "CustomLogs",
                FileName = "custom_log_.txt",
                OutputTemplate =
                    "{Timestamp:HH:mm:ss}|{Level:u1}|{Context}|CUSTOM {Message:lj}{NewLine}{Exception}",
                EnableConsole = true,
                EnableDebug = true,
                EnableFile = true,
                ConsoleLogLevel = LogEventLevel.Information,
                DebugLogLevel = LogEventLevel.Verbose,
                FileLogLevel = LogEventLevel.Warning
            };

            return AppLogger.Build(customConfig);
        }

        private static async Task SimulateExceptionAsync()
        {
            await Task.Delay(100);
            throw new InvalidOperationException("Simulated exception.");
        }
    }
}
