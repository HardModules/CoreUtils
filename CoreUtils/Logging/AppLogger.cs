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
    private static readonly LoggingLevelSwitch ConsoleLevelSwitch = new();
    private static readonly LoggingLevelSwitch DebugLevelSwitch = new();
    private static readonly LoggingLevelSwitch FileLevelSwitch = new();

    /// <summary>
    /// Gets or sets the currently configured logger instance.
    /// </summary>
    public static ILogger Instance { get; private set; } = Build();

    /// <summary>
    /// Builds a logger using the provided logger configuration or creates a new default one if not provided.
    /// </summary>
    /// <param name="cfg">The logger configuration to build with.</param>
    /// <returns>The built logger instance.</returns>
    public static ILogger Build(LoggerConfig cfg = null)
    {
        cfg ??= new LoggerConfig();

        Directory.CreateDirectory(cfg.LogPath);

        ConsoleLevelSwitch.MinimumLevel = cfg.ConsoleLogLevel;
        DebugLevelSwitch.MinimumLevel = cfg.DebugLogLevel;
        FileLevelSwitch.MinimumLevel = cfg.FileLogLevel;

        var serilogCfg = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.WithDemystifiedStackTraces();

        if (cfg.EnableFile)
        {
            serilogCfg
                .WriteTo.File(Path.Combine(cfg.LogPath, cfg.FileName),
                    outputTemplate: cfg.OutputTemplate,
                    rollingInterval: cfg.RollingInterval,
                    retainedFileCountLimit: cfg.RetainedFileCountLimit,
                    levelSwitch: FileLevelSwitch, formatProvider: CultureInfo.InvariantCulture);
        }

        if (cfg.EnableConsole)
        {
            serilogCfg
                .WriteTo.Console(outputTemplate: cfg.OutputTemplate,
                    levelSwitch: ConsoleLevelSwitch, formatProvider: CultureInfo.InvariantCulture);
        }

        if (cfg.EnableDebug)
        {
            serilogCfg
                .WriteTo.Debug(outputTemplate: cfg.OutputTemplate,
                    levelSwitch: DebugLevelSwitch, formatProvider: CultureInfo.InvariantCulture);
        }

        foreach (var sinkConfig in cfg.Sinks)
        {
            serilogCfg.WriteTo.Sink(sinkConfig);
        }

        Instance = serilogCfg
            .CreateLogger()
            .ForContext("Context", cfg.ContextName);

        return Instance;
    }

    /// <summary>
    /// Sets the minimum console logging level.
    /// </summary>
    /// <param name="consoleLevel">The log event level for the console logger.</param>
    public static void SetConsoleMinLevel(LogEventLevel consoleLevel) =>
        ConsoleLevelSwitch.MinimumLevel = consoleLevel;

    /// <summary>
    /// Sets the minimum debug logging level.
    /// </summary>
    /// <param name="consoleLevel">The log event level for the console logger.</param>
    public static void SetDebugMinLevel(LogEventLevel consoleLevel) =>
        DebugLevelSwitch.MinimumLevel = consoleLevel;

    /// <summary>
    /// Sets the minimum file logging level.
    /// </summary>
    /// <param name="fileLevel">The log event level for the file logger.</param>
    public static void SetFileMinLevel(LogEventLevel fileLevel) =>
        FileLevelSwitch.MinimumLevel = fileLevel;

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