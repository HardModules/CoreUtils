using System.Collections.Concurrent;

namespace HardDev.CoreUtils.Config;

/// <summary>
/// Provides a utility for managing configurations throughout the application.
/// </summary>
public static class AppConfig
{
    private static readonly ConcurrentDictionary<string, IConfiguration<object>> s_configurations = new();

    /// <summary>
    /// Gets the instance of the requested configuration type.
    /// If an instance does not exist, creates a new one.
    /// </summary>
    /// <typeparam name="T">The type of configuration to retrieve.</typeparam>
    /// <returns>An instance of the requested configuration type.</returns>
    public static T Get<T>()
        where T : BaseConfiguration<T>, new()
    {
        return s_configurations.GetOrAdd(typeof(T).FullName!, _ => new T()) as T;
    }

    /// <summary>
    /// Gets the instance of the requested configuration type by name.
    /// If an instance does not exist, creates a new one and loads it.
    /// </summary>
    /// <typeparam name="T">The type of configuration to retrieve.</typeparam>
    /// <returns>An instance of the requested configuration type.</returns>
    public static T GetOrLoad<T>()
        where T : BaseConfiguration<T>, new()
    {
        return s_configurations.GetOrAdd(typeof(T).FullName!, _ => new T().Load()) as T;
    }

    /// <summary>
    /// Gets the instance of the requested configuration type by name.
    /// If an instance does not exist, creates a new one and loads it.
    /// </summary>
    /// <typeparam name="T">The type of configuration to retrieve.</typeparam>
    /// <param name="loaded">True if the configuration was loaded successfully, false otherwise.</param>
    /// <returns>An instance of the requested configuration type.</returns>
    public static T GetOrLoad<T>(out bool loaded)
        where T : BaseConfiguration<T>, new()
    {
        string name = typeof(T).FullName!;
        T config;

        if (s_configurations.TryGetValue(name, out IConfiguration<object> value))
        {
            loaded = false;
            config = (T)value;
        }
        else
        {
            config = new T();
            config.Load(out loaded);
            s_configurations.TryAdd(name, config);
        }

        return config;
    }

    /// <summary>
    /// Determines if a configuration of the requested type exists.
    /// </summary>
    /// <typeparam name="T">The type of configuration to check.</typeparam>
    /// <returns>True if the configuration exists, false otherwise.</returns>
    public static bool Contains<T>()
        where T : BaseConfiguration<T>, new()
    {
        return s_configurations.ContainsKey(typeof(T).Name);
    }

    /// <summary>
    /// Removes a configuration of the requested type.
    /// </summary>
    /// <typeparam name="T">The type of configuration to remove.</typeparam>
    /// <returns>True if the configuration was removed, false otherwise.</returns>
    public static bool Remove<T>()
        where T : BaseConfiguration<T>, new()
    {
        return s_configurations.TryRemove(typeof(T).Name, out _);
    }

    /// <summary>
    /// Clears all the configuration instances managed by the AppConfig.
    /// </summary>
    public static void Clear()
    {
        s_configurations.Clear();
    }

    /// <summary>
    /// Loads all the configuration instances managed by the AppConfig.
    /// </summary>
    public static void Load()
    {
        foreach (IConfiguration<object> config in s_configurations.Values)
        {
            config.Load();
        }
    }

    /// <summary>
    /// Saves all the configuration instances managed by the AppConfig.
    /// </summary>
    public static void Save()
    {
        foreach (IConfiguration<object> config in s_configurations.Values)
        {
            config.Save();
        }
    }
}
