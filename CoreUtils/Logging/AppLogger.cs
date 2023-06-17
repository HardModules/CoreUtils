using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace HardDev.CoreUtils.Logging;

/// <summary>
/// AppLogger is a utility class for Serilog logging configuration and management.
/// </summary>
public static class AppLogger
{
    private static readonly LoggingLevelSwitch ConsoleLevelSwitch = new();
    private static readonly LoggingLevelSwitch FileLevelSwitch = new();
    private static readonly object DirectoryCreationLock = new();

    /// <summary>
    /// Gets or sets the currently configured logger instance.
    /// </summary>
    public static ILogger Log { get; private set; } = Configure();

    static AppLogger()
    {
        RegisterGlobalEventHandlers();
    }

    /// <summary>
    /// Configures and applies the given logger configuration or creates a new default one if not provided.
    /// </summary>
    /// <param name="loggerConfig">The logger configuration to apply.</param>
    /// <returns>The configured logger instance.</returns>
    public static ILogger Configure(LoggerConfig loggerConfig = null)
    {
        return Log = Build(loggerConfig ?? new LoggerConfig());
    }

    /// <summary>
    /// Builds a logger using the provided logger configuration.
    /// </summary>
    /// <param name="loggerConfig">The logger configuration to build with.</param>
    /// <returns>The built logger instance.</returns>
    public static ILogger Build(LoggerConfig loggerConfig)
    {
        lock (DirectoryCreationLock)
        {
            Directory.CreateDirectory(loggerConfig.LogDirectory);
        }

        ConsoleLevelSwitch.MinimumLevel = loggerConfig.ConsoleLogLevel;
        FileLevelSwitch.MinimumLevel = loggerConfig.FileLogLevel;

        var configuration = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.WithDemystifiedStackTraces();

        if (loggerConfig.EnableFileLogging)
        {
            configuration = configuration
                .WriteTo.File(Path.Combine(loggerConfig.LogDirectory, loggerConfig.LogFileName),
                    outputTemplate: loggerConfig.OutputTemplate,
                    rollingInterval: loggerConfig.RollingInterval,
                    levelSwitch: FileLevelSwitch);
        }

        if (loggerConfig.EnableConsoleLogging)
        {
            configuration = configuration
                .WriteTo.Console(outputTemplate: loggerConfig.OutputTemplate,
                    levelSwitch: ConsoleLevelSwitch);
        }

        foreach (var sinkConfig in loggerConfig.AdditionalSinks)
        {
            configuration = configuration.WriteTo.Sink(sinkConfig);
        }

        Log = configuration
            .CreateLogger()
            .ForContext("Context", loggerConfig.LoggerContextName);

        return Log;
    }

    /// <summary>
    /// Sets the minimum console logging level.
    /// </summary>
    /// <param name="consoleLevel">The log event level for the console logger.</param>
    public static void SetConsoleMinLevel(LogEventLevel consoleLevel) =>
        ConsoleLevelSwitch.MinimumLevel = consoleLevel;

    /// <summary>
    /// Sets the minimum file logging level.
    /// </summary>
    /// <param name="fileLevel">The log event level for the file logger.</param>
    public static void SetFileMinLevel(LogEventLevel fileLevel) =>
        FileLevelSwitch.MinimumLevel = fileLevel;

    /// <summary>
    /// Creates a logger instance with the specified context name.
    /// </summary>
    /// <param name="name">The context name for the logger.</param>
    /// <returns>A new logger instance with the specified context name.</returns>
    public static ILogger ForName(string name)
    {
        return Log.ForContext("Context", name);
    }

    /// <summary>
    /// Registers handlers for global events such as unhandled exceptions and unobserved task exceptions.
    /// </summary>
    private static void RegisterGlobalEventHandlers()
    {
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
    {
        Log.Fatal((Exception)args.ExceptionObject, "Unhandled exception");
    }

    private static void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs args)
    {
        Log.Fatal(args.Exception, "Unobserved task exception");
        args.SetObserved();
    }
}