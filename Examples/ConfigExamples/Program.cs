using HardDev.CoreUtils.Config;
using HardDev.CoreUtils.Logging;
using Serilog;

namespace HardDev.ConfigExamples;

public static class Program
{
    // Configure the logger for the example class
    private static readonly ILogger Logger = AppLogger.Configure(new LoggerConfig { EnableFileLogging = false });

    public static void Main()
    {
        // Loads SampleConfig and demonstrates its configurations
        Logger.Information("Loading SampleConfig...");

        // Get or create a SampleConfig instance
        var sampleConfig = AppConfig.Get<SampleConfig>();

        // Print the loaded values
        Logger.Information("Loaded values from SampleConfig:");
        Logger.Information("IntegerValue: {IntegerValue}", sampleConfig.IntegerValue);
        Logger.Information("DoubleValue: {DoubleValue}", sampleConfig.DoubleValue);
        Logger.Information("FloatValue: {FloatValue}", sampleConfig.FloatValue);
        Logger.Information("BooleanValue: {BooleanValue}", sampleConfig.BooleanValue);
        Logger.Information("Url: {Url}", sampleConfig.Url);
        Logger.Information("Accounts: {Accounts}", string.Join(", ", sampleConfig.Accounts));
        Logger.Information("Numbers: {Numbers}", string.Join(", ", sampleConfig.Numbers));
        Logger.Information("ExampleDictionary: {ExampleDictionary}", string.Join(", ", sampleConfig.ExampleDictionary));

        // Change some values
        Logger.Information("Changing some values in SampleConfig...");
        sampleConfig.IntegerValue = 999;
        sampleConfig.DoubleValue = 6.28;

        // Validate the configuration 
        Logger.Information("Validating SampleConfig...");
        sampleConfig.EnsureValidProperties();
        
        // Print the changed values
        Logger.Information("Changed values in SampleConfig:");
        Logger.Information("IntegerValue: {IntegerValue}", sampleConfig.IntegerValue);
        Logger.Information("DoubleValue: {DoubleValue}", sampleConfig.DoubleValue);

        // Save the configuration
        Logger.Information("Saving SampleConfig...");
        sampleConfig.Save();

        // Wait for user input to close the console window
        Logger.Information("Press any key to exit...");
        Console.ReadKey();
    }
}