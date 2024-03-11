namespace HardDev.CoreUtils.Config;

/// <summary>
/// Describes the basic actions for managing a configuration.
/// </summary>
public interface IConfiguration
{
    /// <summary>
    /// Loads the configuration data from the file.
    /// </summary>
    void Load();

    /// <summary>
    /// Saves the current configuration data to the file.
    /// </summary>
    void Save();

    /// <summary>
    /// Resets all configuration data to default values.
    /// </summary>
    void Reset();
}