using System.Collections.Concurrent;

namespace HardDev.CoreUtils.Config;

/// <summary>
/// Provides a utility for managing configurations throughout the application.
/// </summary>
public static class AppConfig
{
    private static readonly ConcurrentDictionary<string, IConfiguration> Configurations = new();

    /// <summary>
    /// Gets the instance of the requested configuration type. If an instance does not exist, creates a new one.
    /// </summary>
    /// <typeparam name="T">The type of configuration to retrieve.</typeparam>
    /// <returns>An instance of the requested configuration type.</returns>
    public static T Get<T>() where T : BaseConfiguration<T>, new()
    {
        var configType = typeof(T).Name;
        var config = Configurations.GetOrAdd(configType, _ =>
        {
            var config = new T();
            config.Load();
            return config;
        });
        return (T)config;
    }

    /// <summary>
    /// Clears all the configuration instances managed by the AppConfig.
    /// This may be useful in scenarios where a complete reset of the application state is required,
    /// such as during testing or after a significant change in the application environment.
    /// </summary>
    public static void Clear()
    {
        Configurations.Clear();
    }
}