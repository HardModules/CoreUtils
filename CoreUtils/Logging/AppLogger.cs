using System.Globalization;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace HardDev.CoreUtils.Logging;

/// <summary>
/// AppLogger is a utility class for Serilog logging configuration and management.
/// </summary>
public static class AppLogger
{
    private static readonly LoggingLevelSwitch s_consoleLevelSwitch = new();
    private static readonly LoggingLevelSwitch s_debugLevelSwitch = new();
    private static readonly LoggingLevelSwitch s_fileLevelSwitch = new();

    private static ILogger s_instance;

    /// <summary>
    /// Gets or sets the currently configured logger instance.
    /// </summary>
    public static ILogger Instance
    {
        get { return s_instance ??= Build(); }
        set => s_instance = value;
    }

    /// <summary>
    /// Builds a logger using the provided logger configuration or creates a new default one if not provided.
    /// </summary>
    /// <param name="appLoggerCfg">The logger configuration to build with.</param>
    /// <returns>The built logger instance.</returns>
    public static ILogger Build(LoggerConfig appLoggerCfg = null)
    {
        appLoggerCfg ??= new LoggerConfig();

        s_consoleLevelSwitch.MinimumLevel = appLoggerCfg.ConsoleLogLevel;
        s_debugLevelSwitch.MinimumLevel = appLoggerCfg.DebugLogLevel;
        s_fileLevelSwitch.MinimumLevel = appLoggerCfg.FileLogLevel;

        LoggerConfiguration serilogCfg = new LoggerConfiguration()
            .MinimumLevel.Verbose();

        if (appLoggerCfg.EnableFile)
        {
            serilogCfg
                .WriteTo.File(Path.Combine(appLoggerCfg.LogPath, appLoggerCfg.FileName),
                    outputTemplate: appLoggerCfg.OutputTemplate,
                    rollingInterval: appLoggerCfg.RollingInterval,
                    retainedFileCountLimit: appLoggerCfg.RetainedFileCountLimit,
                    levelSwitch: s_fileLevelSwitch, formatProvider: CultureInfo.InvariantCulture);
        }

        if (appLoggerCfg.EnableConsole)
        {
            serilogCfg
                .WriteTo.Console(outputTemplate: appLoggerCfg.OutputTemplate,
                    levelSwitch: s_consoleLevelSwitch, formatProvider: CultureInfo.InvariantCulture);
        }

        if (appLoggerCfg.EnableDebug)
        {
            serilogCfg
                .WriteTo.Debug(outputTemplate: appLoggerCfg.OutputTemplate,
                    levelSwitch: s_debugLevelSwitch, formatProvider: CultureInfo.InvariantCulture);
        }

        foreach (ILogEventSink sinkConfig in appLoggerCfg.Sinks)
        {
            serilogCfg.WriteTo.Sink(sinkConfig);
        }

        ILogger logger = serilogCfg
            .CreateLogger()
            .ForContext("Context", appLoggerCfg.ContextName);

        return s_instance ??= logger;
    }

    /// <summary>
    /// Sets the minimum console logging level.
    /// </summary>
    /// <param name="consoleLevel">The log event level for the console logger.</param>
    public static void SetConsoleMinLevel(LogEventLevel consoleLevel) =>
        s_consoleLevelSwitch.MinimumLevel = consoleLevel;

    /// <summary>
    /// Sets the minimum debug logging level.
    /// </summary>
    /// <param name="consoleLevel">The log event level for the console logger.</param>
    public static void SetDebugMinLevel(LogEventLevel consoleLevel) =>
        s_debugLevelSwitch.MinimumLevel = consoleLevel;

    /// <summary>
    /// Sets the minimum file logging level.
    /// </summary>
    /// <param name="fileLevel">The log event level for the file logger.</param>
    public static void SetFileMinLevel(LogEventLevel fileLevel) =>
        s_fileLevelSwitch.MinimumLevel = fileLevel;

    /// <summary>
    /// Creates a logger instance with the provided context name.
    /// </summary>
    /// <param name="name">The context name for the logger.</param>
    /// <returns>A new logger instance with the provided context name.</returns>
    public static ILogger ForContext(string name)
    {
        return Instance.ForContext("Context", name);
    }

    /// <summary>
    /// Creates a logger instance with the provided context type.
    /// </summary>
    /// <typeparam name="T">The context type for the logger.</typeparam>
    /// <returns>A new logger instance with the provided context type.</returns>
    public static ILogger ForContext<T>()
    {
        return Instance.ForContext("Context", typeof(T).Name);
    }

    /// <summary>
    /// Registers handlers for global events such as unhandled exceptions and unobserved task exceptions.
    /// </summary>
    public static void RegisterGlobalEventHandlers()
    {
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    /// <summary>
    /// Unregisters handlers for global events such as unhandled exceptions and unobserved task exceptions.
    /// </summary>
    public static void UnregisterGlobalEventHandlers()
    {
        AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
        TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
    {
        Log.Fatal((Exception)args.ExceptionObject, "Unhandled exception");
    }

    private static void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs args)
    {
        Log.Fatal(args.Exception, "Unobserved task exception");
    }
}
