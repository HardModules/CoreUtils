# HardDev.CoreUtils

[![NuGet](https://img.shields.io/nuget/v/HardDev.CoreUtils.svg)](https://www.nuget.org/packages/HardDev.CoreUtils)

HardDev.CoreUtils is a utility library providing core functionality for .NET applications. It includes advanced
configuration handling, logging, and more. This library is designed to simplify common tasks and help developers create
cleaner and more efficient code.

## Features

- Advanced configuration handling with support for JSON files and built-in default values and validation.
- Native JSON serialization support using source generators for better performance.
- Easy-to-use logging setup built on top of the Serilog library.
- Highly customizable logger configuration with adjustable log levels, output format, and more.
- Supports .NET Standard 2.0, .NET Standard 2.1 and .NET 9.0 target frameworks

## Getting Started

Install the latest version of the HardDev.CoreUtils library from NuGet.

```sh
dotnet add package HardDev.CoreUtils
```

## Usage

### Configuration

The HardDev.CoreUtils configuration system treats configurations as singletons, providing consistently updated values
across your application.

Creating a Configuration file:

```csharp
using System.ComponentModel.DataAnnotations;
using HardDev.CoreUtils.Config;

namespace HardDev.ConfigExamples;

/// <summary>
/// A sample configuration to show how to derive from BaseConfiguration and use default values and validations.
/// </summary>
public sealed class SampleConfig() : BaseConfiguration<SampleConfig>("Configs/SampleConfig.json")
{
    /// <summary>
    /// Gets or sets an integer value.
    /// </summary>
    [Range(1, 100)]
    public int IntegerValue { get; set; } = 42;

    /// <summary>
    /// Gets or sets a double value.
    /// </summary>
    public double DoubleValue { get; set; } = 3.14;

    /// <summary>
    /// Gets or sets a float value.
    /// </summary>
    public float FloatValue { get; set; } = 123.456f;

    /// <summary>
    /// Gets or sets a boolean value.
    /// </summary>
    public bool BooleanValue { get; set; } = true;

    /// <summary>
    /// Gets or sets a URL as a string.
    /// </summary>
    [Url(ErrorMessage = "Invalid URL format")]
    public string Url { get; set; } = "http://default_url.com";

    /// <summary>
    /// Gets or sets a read-only list of strings representing accounts.
    /// </summary>
    [Required(ErrorMessage = "Accounts cannot be null"),
     MinLength(1, ErrorMessage = "Accounts cannot be empty")]
    public IReadOnlyList<string> Accounts { get; set; } = new List<string> { "Account1", "Account2", "Account3" };

    /// <summary>
    /// Gets or sets a read-only list of integers representing numbers.
    /// </summary>
    [Required(ErrorMessage = "Numbers cannot be null"),
     MinLength(1, ErrorMessage = "Numbers cannot be empty")]
    public IReadOnlyList<int> Numbers { get; set; } = new List<int> { 1, 2, 3 };

    /// <summary>
    /// Gets or sets a dictionary with string keys and integer values.
    /// </summary>
    [Required(ErrorMessage = "Example dictionary cannot be null"), MinLength(1, ErrorMessage = "ExampleDictionary cannot be empty")]
    public IDictionary<string, int> ExampleDictionary { get; set; } = new Dictionary<string, int> { { "Key1", 1 }, { "Key2", 2 } };
}
```

Using a Configuration in your application:

``` csharp
using HardDev.CoreUtils.Config;
using HardDev.CoreUtils.Logging;
using Serilog;

namespace HardDev.ConfigExamples;

public static class Program
{
    // Configure the logger for the example class
    private static readonly ILogger Logger = AppLogger.Build(new LoggerConfig { EnableFile = false });

    public static void Main()
    {
        // Get or load a SampleConfig instance
        var sampleConfig = AppConfig.GetOrLoad<SampleConfig>();

        Logger.Information("Loaded values from SampleConfig:");
        Logger.Information("IntegerValue: {IntegerValue}", sampleConfig.IntegerValue);
        Logger.Information("DoubleValue: {DoubleValue}", sampleConfig.DoubleValue);
        Logger.Information("FloatValue: {FloatValue}", sampleConfig.FloatValue);
        Logger.Information("BooleanValue: {BooleanValue}", sampleConfig.BooleanValue);
        Logger.Information("Url: {Url}", sampleConfig.Url);
        Logger.Information("Accounts: {Accounts}", string.Join(", ", sampleConfig.Accounts));
        Logger.Information("Numbers: {Numbers}", string.Join(", ", sampleConfig.Numbers));
        Logger.Information("ExampleDictionary: {ExampleDictionary}", string.Join(", ", sampleConfig.ExampleDictionary));

        Logger.Information("Changing some values in SampleConfig");
        sampleConfig.IntegerValue = 999;
        sampleConfig.DoubleValue = 6.28;

        Logger.Information("Validating SampleConfig");
        sampleConfig.EnsureValidProperties();

        Logger.Information("Changed values in SampleConfig:");
        Logger.Information("IntegerValue: {IntegerValue}", sampleConfig.IntegerValue);
        Logger.Information("DoubleValue: {DoubleValue}", sampleConfig.DoubleValue);

        Logger.Information("Saving SampleConfig...");
        sampleConfig.Save();

        Logger.Information("Press any key to exit...");
        Console.ReadKey();
    }
}
```

### Logging

HardDev.CoreUtils.Logging provides an easy-to-use logger configuration and management built on top of the Serilog
library.

Here's an example of setting up the logger and logging a message:

``` csharp
using HardDev.CoreUtils.Logging;
using Serilog.Events;

// Configure the logger
var cfg = new LoggerConfig
{
    LogPath = "Logs",
    EnableConsole = true,
    EnableFile = true,
    ConsoleLogLevel = LogEventLevel.Debug,
    FileLogLevel = LogEventLevel.Verbose,
};
var logger = AppLogger.Build(cfg);

// Log a message
logger.Information("Hello, World!");
```

More examples of using the logger:

1. Log an error message with exception:

``` csharp
try
{
    // Some code that might throw an exception
}
catch (Exception ex)
{
    AppLogger.Instance.Error(ex, "An error occurred");
}
```

2. Log a warning message with properties:

``` csharp
int currentUsers = 50;
int maxUsers = 100;
AppLogger.Instance.Warning("Current users reached {CurrentUsers} out of {MaxUsers}", currentUsers, maxUsers);
```

3. Scoped logger with context:

```csharp
var scopedLogger = AppLogger.ForName("ScopedContext");
scopedLogger.Information("This message has a custom context.");
```


## License

This project is licensed under the [MIT License](https://github.com/HardModules/CoreUtils/blob/main/LICENSE).
