using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using HardDev.CoreUtils.Logging;
using Serilog;

namespace HardDev.CoreUtils.Config;

/// <summary>
/// A base class for handling configuration files.
/// </summary>
/// <typeparam name="T">The type of configuration derived from this base class.</typeparam>
public abstract class BaseConfiguration<T> : IConfiguration where T : BaseConfiguration<T>
{
    /// <summary>
    /// Gets the path to the configuration file.
    /// </summary>
    [JsonIgnore]
    public string ConfigPath { get; }

    private readonly ILogger _logger = AppLogger.ForName(typeof(T).Name);

    /// <summary>
    /// Initializes a new instance of the BaseConfiguration class.
    /// </summary>
    /// <param name="configPath">The path to the configuration file.</param>
    protected BaseConfiguration(string configPath)
    {
        ConfigPath = configPath;
    }

    /// <summary>
    /// Loads the configuration data from the file.
    /// </summary>
    public void Load()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                var content = File.ReadAllText(ConfigPath);

                if (!string.IsNullOrEmpty(content))
                {
                    try
                    {
                        Populate(JsonSerializer.Deserialize<T>(content));
                        EnsureValidProperties();
                    }
                    catch (JsonException)
                    {
                        _logger.Warning(
                            "Configuration file contains invalid JSON. Using default values and updating the file");
                        Reset();
                    }
                }
                else
                {
                    _logger.Warning("Configuration file is empty. Using default values and updating the file");
                    Reset();
                }
            }
            else
            {
                _logger.Warning("Configuration file not found, creating a new one with default values");
                Reset();
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error occurred while loading the configuration");
        }
    }

    /// <summary>
    /// Saves the current configuration data to the file.
    /// </summary>
    public void Save()
    {
        try
        {
            var directoryPath = Path.GetDirectoryName(ConfigPath);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var json = JsonSerializer.Serialize(this as T);
            File.WriteAllText(ConfigPath, json);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error occurred while saving the configuration");
        }
    }

    /// <summary>
    /// Resets all configuration data to default values.
    /// </summary>
    public void Reset()
    {
        Populate(Activator.CreateInstance<T>());
    }

    /// <summary>
    /// Validates and corrects the properties of the configuration object by setting default values for invalid properties.
    /// </summary>
    /// <returns>True if any properties were corrected, otherwise false.</returns>
    public bool EnsureValidProperties()
    {
        var changesMade = false;
        var defaultInstance = Activator.CreateInstance<T>();

        foreach (var prop in typeof(T).GetProperties().Where(p => p.CanRead && p.CanWrite))
        {
            try
            {
                var validationContext = new ValidationContext(this) { MemberName = prop.Name };
                var results = new List<ValidationResult>();

                var isValid = Validator.TryValidateProperty(prop.GetValue(this), validationContext, results);
                if (isValid)
                    continue;

                _logger.Warning("Validation error for property '{PropName}':", prop.Name);
                foreach (var validationResult in results)
                {
                    _logger.Warning(" - {ErrorMessage}", validationResult.ErrorMessage);
                }

                prop.SetValue(this, prop.GetValue(defaultInstance));
                _logger.Information("Set default value for property '{PropName}'", prop.Name);
                changesMade = true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred during configuration validation");
            }
        }

        return changesMade;
    }

    /// <summary>
    /// Populates the properties of the target object with the values of the properties of the source object.
    /// </summary>
    /// <param name="target"></param>
    private void Populate(T target)
    {
        foreach (var prop in typeof(T).GetProperties().Where(p => p.CanRead && p.CanWrite))
        {
            prop.SetValue(this, prop.GetValue(target));
        }
    }
}