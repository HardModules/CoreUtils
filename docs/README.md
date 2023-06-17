# HardDev.CoreUtils

[![NuGet](https://img.shields.io/nuget/v/HardDev.CoreUtils.svg)](https://www.nuget.org/packages/HardDev.CoreUtils)

HardDev.CoreUtils is a utility library providing core functionality for .NET applications. It includes advanced
configuration handling, logging, and more. This library is designed to simplify common tasks and help developers create
cleaner and more efficient code.

## Features

- Advanced configuration handling with support for JSON files and built-in default values and validation.
- Easy-to-use logging setup built on top of the Serilog library.
- Highly customizable logger configuration with adjustable log levels, output format, and more.
- Supports .NET 6.0 and .NET 7.0 target frameworks

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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using HardDev.CoreUtils.Config;

[DataContract]
public class SampleConfig : BaseConfiguration<SampleConfig>
{
    [DataMember, DefaultValue(42)]
    public int IntegerValue { get; set; }

    [DataMember, DefaultValue("http://default_url.com"), Url(ErrorMessage = "Invalid URL format")]
    public string Url { get; set; } 

    [DataMember, CollectionDefaultValue(typeof(string[]), "Acc1", "Acc2"), Required(ErrorMessage = "Accounts cannot be null"),
     MinLength(1, ErrorMessage = "Accounts cannot be empty")]
    public IReadOnlyList<string> Accounts { get; set; }

    [DataMember, CollectionDefaultValue(typeof(List<int>), 1, 2, 3), Required(ErrorMessage = "Numbers cannot be null"),
     MinLength(1, ErrorMessage = "Numbers cannot be empty")]
    public IReadOnlyList<int> Numbers { get; set; }

    [DataMember, CollectionDefaultValue(typeof(Dictionary<string, int>), "Key1", 1, "Key2", 2),
     Required(ErrorMessage = "Example dictionary cannot be null"), MinLength(1, ErrorMessage = "ExampleDictionary cannot be empty")]
    public IDictionary<string, int> ExampleDictionary { get; set; }

    public SampleConfig() : base("Configs/SampleConfig.json")
    {
    }
}
```

Using a Configuration in your application:

``` csharp
// Loads SampleConfig and demonstrates its configurations
Logger.Information("Loading SampleConfig...");

// Get or create a SampleConfig instance
var sampleConfig = AppConfig.Get<SampleConfig>();

// Print the loaded values
Logger.Information("Loaded values from SampleConfig:");
Logger.Information("IntegerValue: {IntegerValue}", sampleConfig.IntegerValue);
Logger.Information("Url: {Url}", sampleConfig.Url);
Logger.Information("Accounts: {Accounts}", string.Join(", ", sampleConfig.Accounts));
Logger.Information("Numbers: {Numbers}", string.Join(", ", sampleConfig.Numbers));
Logger.Information("ExampleDictionary: {ExampleDictionary}", string.Join(", ", sampleConfig.ExampleDictionary));

// Change some values
Logger.Information("Changing some values in SampleConfig...");
sampleConfig.IntegerValue = 24;

// Save the configuration asynchronously
Logger.Information("Saving SampleConfig...");
sampleConfig.Save();
```

### Logging

HardDev.CoreUtils.Logging provides an easy-to-use logger configuration and management built on top of the Serilog
library.

Here's an example of setting up the logger and logging a message:

``` csharp
using HardDev.CoreUtils.Logging;
using Serilog.Events;

// Configure the logger
var loggerConfig = new AppLoggerConfig
{
    LogDirectory = "Logs",
    EnableConsoleLogging = true,
    EnableFileLogging = true,
    ConsoleLogLevel = LogEventLevel.Debug,
    FileLogLevel = LogEventLevel.Verbose,
};
AppLogger.Configure(loggerConfig);

// Log a message
AppLogger.Log.Information("Hello, World!");
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
    AppLogger.Log.Error(ex, "An error occurred");
}
```

2. Log a warning message with properties:

``` csharp
int currentUsers = 50;
int maxUsers = 100;
AppLogger.Log.Warning("Current users reached {CurrentUsers} out of {MaxUsers}", currentUsers, maxUsers);
```

3. Scoped logger with context:

```csharp
var scopedLogger = AppLogger.ForName("ScopedContext");
scopedLogger.Information("This message has a custom context.");
```

## Build

HardDev.CoreUtils supports the following target frameworks:

- .NET Standard 2.0
- .NET 6.0
- .NET 7.0

## License

This project is licensed under the [MIT License](https://github.com/HardDev/CoreUtils/blob/main/LICENSE).