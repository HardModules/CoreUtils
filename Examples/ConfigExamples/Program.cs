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
        // Get or create a SampleConfig instance
        var sampleConfig = AppConfig.Get<SampleConfig>();

        Logger.Information("Loaded configuration from file");
        sampleConfig.Load();

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