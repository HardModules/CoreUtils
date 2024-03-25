using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using HardDev.CoreUtils.Logging;
using Serilog;

namespace HardDev.CoreUtils.Config;

/// <summary>
/// A base class for handling configuration files.
/// </summary>
/// <typeparam name="T">The type of configuration derived from this base class.</typeparam>
public abstract class BaseConfiguration<T> : IConfiguration<T> where T : BaseConfiguration<T>
{
    /// <summary>
    /// Gets the path to the configuration file.
    /// </summary>
    [JsonIgnore]
    public string ConfigPath { get; }

    /// <summary>
    /// Gets or sets the JSON serialization options.
    /// </summary>
    [JsonIgnore]
    public JsonSerializerOptions Options { get; set; }

    private readonly ILogger _logger = AppLogger.ForContext<T>();

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
    /// <returns>The current instance of the configuration.</returns>
    public T Load()
    {
        return Load(out _);
    }

    /// <summary>
    /// Loads the configuration data from the file.
    /// </summary>
    /// <returns>The current instance of the configuration.</returns>
    /// <param name="loaded">True if the configuration was loaded successfully, false otherwise.</param>
    public T Load(out bool loaded)
    {
        loaded = false;

        try
        {
            if (File.Exists(ConfigPath))
            {
                var content = File.ReadAllText(ConfigPath);

                if (!string.IsNullOrEmpty(content))
                {
                    try
                    {
                        Populate(JsonSerializer.Deserialize<T>(content, Options));
                        EnsureValidProperties();
                        loaded = true;
                    }
                    catch (JsonException)
                    {
                        _logger.Warning("Configuration file contains invalid JSON");
                        Reset();
                    }
                }
                else
                {
                    _logger.Warning("Configuration file is empty");
                    Reset();
                }
            }
            else
            {
                _logger.Warning("Configuration file not found");
                Reset();
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error occurred while loading the configuration");
            Reset();
        }

        return this as T;
    }

    /// <summary>
    /// Saves the current configuration data to the file.
    /// </summary>
    /// <returns>The current instance of the configuration.</returns>
    public T Save()
    {
        return Save(out _);
    }

    /// <summary>
    /// Saves the current configuration data to the file.
    /// </summary>
    /// <returns>The current instance of the configuration.</returns>
    /// <param name="saved">True if the configuration was saved successfully, false otherwise.</param>
    public T Save(out bool saved)
    {
        saved = false;

        try
        {
            var directoryPath = Path.GetDirectoryName(ConfigPath);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var json = JsonSerializer.Serialize(this as T, Options);
            File.WriteAllText(ConfigPath, json);
            saved = true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error occurred while saving the configuration");
        }

        return this as T;
    }

    /// <summary>
    /// Resets all configuration data to default values.
    /// </summary>
    /// <returns>The current instance of the configuration.</returns>
    public T Reset()
    {
        Populate(Activator.CreateInstance<T>());
        return this as T;
    }

    /// <summary>
    /// Validates and corrects the properties of the configuration object by setting default values for invalid properties.
    /// </summary>
    /// <returns>True if any properties were corrected, otherwise false.</returns>
    public bool EnsureValidProperties()
    {
        var changesMade = false;
        var defaultInstance = Activator.CreateInstance<T>();

        foreach (var prop in typeof(T).GetProperties()
                     .Where(p => p.CanRead && p.CanWrite && p.GetCustomAttribute<JsonIgnoreAttribute>() == null))
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
                _logger.Warning("Set default value for property '{PropName}'", prop.Name);
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
