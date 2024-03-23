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
    /// Loads the configuration data from the file.
    /// </summary>
    /// <param name="loaded">True if the configuration was loaded successfully, false otherwise.</param>
    /// <returns>The current instance of the configuration.</returns>
    T Load(out bool loaded);

    /// <summary>
    /// Saves the current configuration data to the file.
    /// </summary>
    /// <returns>The current instance of the configuration.</returns>
    T Save();

    /// <summary>
    /// Saves the current configuration data to the file.
    /// </summary>
    /// <param name="saved">True if the configuration was saved successfully, false otherwise.</param>
    /// <returns>The current instance of the configuration.</returns>
    T Save(out bool saved);

    /// <summary>
    /// Resets all configuration data to default values.
    /// </summary>
    /// <returns>The current instance of the configuration.</returns>
    T Reset();
}
