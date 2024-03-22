namespace HardDev.CoreUtils.Config;

/// <summary>
/// Describes the basic actions for managing a configuration.
/// </summary>
public interface IConfiguration<out T>
{
    /// <summary>
    /// Loads the configuration data from the file.
    /// </summary>
    /// <returns>The current instance of the configuration.</returns>
    T Load();

    /// <summary>
    /// Saves the current configuration data to the file.
    /// </summary>
    /// <returns>The current instance of the configuration.</returns>
    T Save();

    /// <summary>
    /// Resets all configuration data to default values.
    /// </summary>
    /// <returns>The current instance of the configuration.</returns>
    T Reset();
}
