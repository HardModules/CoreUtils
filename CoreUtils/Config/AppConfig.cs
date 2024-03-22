using System.Collections.Concurrent;

namespace HardDev.CoreUtils.Config;

/// <summary>
/// Provides a utility for managing configurations throughout the application.
/// </summary>
public static class AppConfig
{
    private static readonly ConcurrentDictionary<string, IConfiguration<object>> _configurations = new();

    /// <summary>
    /// Gets the instance of the requested configuration type.
    /// If an instance does not exist, creates a new one.
    /// </summary>
    /// <typeparam name="T">The type of configuration to retrieve.</typeparam>
    /// <returns>An instance of the requested configuration type.</returns>
    public static T Get<T>() where T : BaseConfiguration<T>, new()
    {
        return _configurations.GetOrAdd(typeof(T).FullName!, _ => new T()) as T;
    }

    /// <summary>
    /// Gets the instance of the requested configuration type by name.
    /// If an instance does not exist, creates a new one and loads it.
    /// </summary>
    /// <typeparam name="T">The type of configuration to retrieve.</typeparam>
    /// <returns>An instance of the requested configuration type.</returns>
    public static T GetOrLoad<T>() where T : BaseConfiguration<T>, new()
    {
        return _configurations.GetOrAdd(typeof(T).FullName!, _ => new T().Load()) as T;
    }

    /// <summary>
    /// Determines if a configuration of the requested type exists.
    /// </summary>
    /// <typeparam name="T">The type of configuration to check.</typeparam>
    /// <returns>True if the configuration exists, false otherwise.</returns>
    public static bool Contains<T>() where T : BaseConfiguration<T>, new()
    {
        return _configurations.ContainsKey(typeof(T).Name);
    }

    /// <summary>
    /// Removes a configuration of the requested type.
    /// </summary>
    /// <typeparam name="T">The type of configuration to remove.</typeparam>
    /// <returns>True if the configuration was removed, false otherwise.</returns>
    public static bool Remove<T>() where T : BaseConfiguration<T>, new()
    {
        return _configurations.TryRemove(typeof(T).Name, out _);
    }

    /// <summary>
    /// Clears all the configuration instances managed by the AppConfig.
    /// </summary>
    public static void Clear()
    {
        _configurations.Clear();
    }

    /// <summary>
    /// Loads all the configuration instances managed by the AppConfig.
    /// </summary>
    public static void Load()
    {
        foreach (var config in _configurations.Values)
        {
            config.Load();
        }
    }

    /// <summary>
    /// Saves all the configuration instances managed by the AppConfig.
    /// </summary>
    public static void Save()
    {
        foreach (var config in _configurations.Values)
        {
            config.Save();
        }
    }
}
