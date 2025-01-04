using HardDev.CoreUtils.Config;
using HardDev.CoreUtils.Logging;
using Serilog;

namespace HardDev.ConfigExamples;

public static class Program
{
    // Configure the logger for the example class
    private static readonly ILogger s_logger = AppLogger.Build(new LoggerConfig { EnableFile = false });

    public static void Main()
    {
        // Get or load a SampleConfig instance
        SampleConfig sampleConfig = AppConfig.GetOrLoad<SampleConfig>();

        s_logger.Information("Loaded values from SampleConfig:");
        s_logger.Information("IntegerValue: {IntegerValue}", sampleConfig.IntegerValue);
        s_logger.Information("DoubleValue: {DoubleValue}", sampleConfig.DoubleValue);
        s_logger.Information("FloatValue: {FloatValue}", sampleConfig.FloatValue);
        s_logger.Information("BooleanValue: {BooleanValue}", sampleConfig.BooleanValue);
        s_logger.Information("Url: {Url}", sampleConfig.Url);
        s_logger.Information("Accounts: {Accounts}", string.Join(", ", sampleConfig.Accounts));
        s_logger.Information("Numbers: {Numbers}", string.Join(", ", sampleConfig.Numbers));
        s_logger.Information("ExampleDictionary: {ExampleDictionary}",
            string.Join(", ", sampleConfig.ExampleDictionary));

        s_logger.Information("Changing some values in SampleConfig");
        sampleConfig.IntegerValue = 999;
        sampleConfig.DoubleValue = 6.28;

        s_logger.Information("Validating SampleConfig");
        sampleConfig.EnsureValidProperties();

        s_logger.Information("Changed values in SampleConfig:");
        s_logger.Information("IntegerValue: {IntegerValue}", sampleConfig.IntegerValue);
        s_logger.Information("DoubleValue: {DoubleValue}", sampleConfig.DoubleValue);

        s_logger.Information("Saving SampleConfig...");
        sampleConfig.Save();

        s_logger.Information("Press any key to exit...");
        Console.ReadKey();
    }
}
