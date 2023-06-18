using HardDev.CoreUtils.Logging;
using Serilog;
using Serilog.Events;

namespace HardDev.LoggingExamples
{
    public static class Program
    {
        // A default logger configuration, useful for most cases
        private static readonly ILogger DefaultLogger = AppLogger.Log;

        // A custom logger tailored to specific needs
        private static readonly ILogger CustomLogger = CreateCustomLogger();

        public static async Task Main()
        {
            // Examples of logging messages at various levels using the default logger
            DefaultLogger.Verbose("This is a verbose log message");
            DefaultLogger.Debug("This is a debug log message");
            DefaultLogger.Information("This is an information log message");
            DefaultLogger.Warning("This is a warning log message");
            DefaultLogger.Error("This is an error log message");
            DefaultLogger.Fatal("This is a fatal log message");

            try
            {
                await SimulateExceptionAsync();
            }
            catch (Exception ex)
            {
                DefaultLogger.Error(ex, "An exception occurred");
            }

            // Examples of logging messages with custom logger
            CustomLogger.Information("Custom logger: This is an information log message");
            CustomLogger.Warning("Custom logger: This is a warning log message");
        }

        private static ILogger CreateCustomLogger()
        {
            var customConfig = new LoggerConfig
            {
                LoggerContextName = "CustomContext",
                LogDirectory = "CustomLogs",
                LogFileName = "custom_log_.txt",
                OutputTemplate = "{Timestamp:HH:mm:ss}|{Level:u1}|{Context}|CUSTOM {Message:lj}{NewLine}{Exception}",
                EnableConsoleLogging = true,
                EnableFileLogging = true,
                ConsoleLogLevel = LogEventLevel.Information,
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