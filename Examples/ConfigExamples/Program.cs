using HardDev.CoreUtils.Config;
using HardDev.CoreUtils.Logging;
using Serilog;

namespace HardDev.ConfigExamples;

public static class Program
{
    // Configure the logger for the example class
    private static readonly ILogger _logger = AppLogger.Build(new LoggerConfig { EnableFile = false });

    public static void Main()
    {
        // Get or load a SampleConfig instance
        var sampleConfig = AppConfig.GetOrLoad<SampleConfig>();

        _logger.Information("Loaded values from SampleConfig:");
        _logger.Information("IntegerValue: {IntegerValue}", sampleConfig.IntegerValue);
        _logger.Information("DoubleValue: {DoubleValue}", sampleConfig.DoubleValue);
        _logger.Information("FloatValue: {FloatValue}", sampleConfig.FloatValue);
        _logger.Information("BooleanValue: {BooleanValue}", sampleConfig.BooleanValue);
        _logger.Information("Url: {Url}", sampleConfig.Url);
        _logger.Information("Accounts: {Accounts}", string.Join(", ", sampleConfig.Accounts));
        _logger.Information("Numbers: {Numbers}", string.Join(", ", sampleConfig.Numbers));
        _logger.Information("ExampleDictionary: {ExampleDictionary}",
            string.Join(", ", sampleConfig.ExampleDictionary));

        _logger.Information("Changing some values in SampleConfig");
        sampleConfig.IntegerValue = 999;
        sampleConfig.DoubleValue = 6.28;

        _logger.Information("Validating SampleConfig");
        sampleConfig.EnsureValidProperties();

        _logger.Information("Changed values in SampleConfig:");
        _logger.Information("IntegerValue: {IntegerValue}", sampleConfig.IntegerValue);
        _logger.Information("DoubleValue: {DoubleValue}", sampleConfig.DoubleValue);

        _logger.Information("Saving SampleConfig...");
        sampleConfig.Save();

        _logger.Information("Press any key to exit...");
        Console.ReadKey();
    }
}
