using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using HardDev.CoreUtils.Logging;
using Newtonsoft.Json;
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
    public string ConfigPath { get; }

    private readonly SemaphoreSlim _configLock = new(1);
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
        _configLock.Wait();
        var loaded = false;
        var needSave = false;

        try
        {
            if (File.Exists(ConfigPath))
            {
                var json = File.ReadAllText(ConfigPath);

                if (!string.IsNullOrEmpty(json))
                {
                    try
                    {
                        ClearCollections();

                        using var stringReader = new StringReader(json);
                        using var jsonReader = new JsonTextReader(stringReader);

                        var serializer = new JsonSerializer();
                        serializer.Populate(jsonReader, this);
                        jsonReader.Close();
                        stringReader.Close();

                        loaded = true;
                    }
                    catch (JsonException)
                    {
                        _logger.Warning("Configuration file contains invalid JSON. Using default values and updating the file");
                        SetDefaultValues();
                        needSave = true;
                    }
                }
                else
                {
                    _logger.Warning("Configuration file is empty. Using default values and updating the file");
                    SetDefaultValues();
                    needSave = true;
                }
            }
            else
            {
                _logger.Warning("Configuration file not found, creating a new one with default values");
                SetDefaultValues();
                needSave = true;
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error occurred while loading the configuration");
        }
        finally
        {
            _configLock.Release();
        }

        if (loaded)
        {
            if (ValidateProperties())
            {
                needSave = true;
            }
        }

        if (needSave)
            Save();
    }

    /// <summary>
    /// Saves the current configuration data to the file.
    /// </summary>
    public void Save()
    {
        _configLock.Wait();
        try
        {
            var directoryPath = Path.GetDirectoryName(ConfigPath);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var settings = new JsonSerializerSettings { Formatting = Formatting.Indented };
            var json = JsonConvert.SerializeObject((T)this, settings);
            File.WriteAllText(ConfigPath, json);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error occurred while saving the configuration");
        }
        finally
        {
            _configLock.Release();
        }
    }

    /// <summary>
    /// Resets all configuration data to default values and saves the changes.
    /// </summary>
    public void Reset()
    {
        SetDefaultValues();
        Save();
    }

    private void ClearCollections()
    {
        foreach (var prop in typeof(T).GetProperties().Where(p => p.CanRead && p.CanWrite && p.PropertyType.GetInterfaces().Any(i =>
                     i.IsGenericType && (i.GetGenericTypeDefinition() == typeof(ICollection<>) ||
                                         i.GetGenericTypeDefinition() == typeof(IDictionary<,>)))))
        {
            if (prop.PropertyType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>)))
            {
                var collection = prop.GetValue(this) as IList;
                collection?.Clear();
            }
            else if (prop.PropertyType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
            {
                var dictionary = prop.GetValue(this) as IDictionary;
                dictionary?.Clear();
            }
        }
    }

    private void SetDefaultValues()
    {
        foreach (var prop in typeof(T).GetProperties().Where(p => p.CanRead && p.CanWrite))
        {
            try
            {
                var defaultValueAttr = prop.GetCustomAttributes(typeof(DefaultValueAttribute), true).OfType<DefaultValueAttribute>()
                    .FirstOrDefault();

                if (defaultValueAttr != null)
                {
                    prop.SetValue(this, defaultValueAttr.Value);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error occurred while setting default values");
            }
        }
    }

    private bool ValidateProperties()
    {
        var needSave = false;

        foreach (var prop in typeof(T).GetProperties().Where(p => p.CanRead && p.CanWrite))
        {
            try
            {
                var validationContext = new ValidationContext(this) { MemberName = prop.Name };
                var results = new List<ValidationResult>();

                var isValid = Validator.TryValidateProperty(prop.GetValue(this), validationContext, results);

                if (!isValid)
                {
                    _logger.Warning("Validation error for the property value '{PropName}':", prop.Name);
                    foreach (var validationResult in results)
                    {
                        _logger.Warning("  - {ErrorMessage}", validationResult.ErrorMessage);
                    }

                    var defaultValueAttr = prop.GetCustomAttributes(typeof(DefaultValueAttribute), true).OfType<DefaultValueAttribute>()
                        .FirstOrDefault();

                    if (defaultValueAttr != null)
                    {
                        prop.SetValue(this, defaultValueAttr.Value);
                        needSave = true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error occurred while validating the configuration");
            }
        }

        return needSave;
    }
}