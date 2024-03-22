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
    private static readonly LoggingLevelSwitch _consoleLevelSwitch = new();
    private static readonly LoggingLevelSwitch _debugLevelSwitch = new();
    private static readonly LoggingLevelSwitch _fileLevelSwitch = new();

    private static ILogger _instance;

    /// <summary>
    /// Gets or sets the currently configured logger instance.
    /// </summary>
    public static ILogger Instance
    {
        get { return _instance ??= Build(); }
        set => _instance = value;
    }

    /// <summary>
    /// Builds a logger using the provided logger configuration or creates a new default one if not provided.
    /// </summary>
    /// <param name="appLoggerCfg">The logger configuration to build with.</param>
    /// <returns>The built logger instance.</returns>
    public static ILogger Build(LoggerConfig appLoggerCfg = null)
    {
        appLoggerCfg ??= new LoggerConfig();

        Directory.CreateDirectory(appLoggerCfg.LogPath);

        _consoleLevelSwitch.MinimumLevel = appLoggerCfg.ConsoleLogLevel;
        _debugLevelSwitch.MinimumLevel = appLoggerCfg.DebugLogLevel;
        _fileLevelSwitch.MinimumLevel = appLoggerCfg.FileLogLevel;

        var serilogCfg = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.WithDemystifiedStackTraces();

        if (appLoggerCfg.EnableFile)
        {
            serilogCfg
                .WriteTo.File(Path.Combine(appLoggerCfg.LogPath, appLoggerCfg.FileName),
                    outputTemplate: appLoggerCfg.OutputTemplate,
                    rollingInterval: appLoggerCfg.RollingInterval,
                    retainedFileCountLimit: appLoggerCfg.RetainedFileCountLimit,
                    levelSwitch: _fileLevelSwitch, formatProvider: CultureInfo.InvariantCulture);
        }

        if (appLoggerCfg.EnableConsole)
        {
            serilogCfg
                .WriteTo.Console(outputTemplate: appLoggerCfg.OutputTemplate,
                    levelSwitch: _consoleLevelSwitch, formatProvider: CultureInfo.InvariantCulture);
        }

        if (appLoggerCfg.EnableDebug)
        {
            serilogCfg
                .WriteTo.Debug(outputTemplate: appLoggerCfg.OutputTemplate,
                    levelSwitch: _debugLevelSwitch, formatProvider: CultureInfo.InvariantCulture);
        }

        foreach (var sinkConfig in appLoggerCfg.Sinks)
        {
            serilogCfg.WriteTo.Sink(sinkConfig);
        }

        return serilogCfg
            .CreateLogger()
            .ForContext("Context", appLoggerCfg.ContextName);
    }

    /// <summary>
    /// Sets the minimum console logging level.
    /// </summary>
    /// <param name="consoleLevel">The log event level for the console logger.</param>
    public static void SetConsoleMinLevel(LogEventLevel consoleLevel) =>
        _consoleLevelSwitch.MinimumLevel = consoleLevel;

    /// <summary>
    /// Sets the minimum debug logging level.
    /// </summary>
    /// <param name="consoleLevel">The log event level for the console logger.</param>
    public static void SetDebugMinLevel(LogEventLevel consoleLevel) =>
        _debugLevelSwitch.MinimumLevel = consoleLevel;

    /// <summary>
    /// Sets the minimum file logging level.
    /// </summary>
    /// <param name="fileLevel">The log event level for the file logger.</param>
    public static void SetFileMinLevel(LogEventLevel fileLevel) =>
        _fileLevelSwitch.MinimumLevel = fileLevel;

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
