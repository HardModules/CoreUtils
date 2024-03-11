using System.ComponentModel.DataAnnotations;
using HardDev.CoreUtils.Config;

namespace HardDev.ConfigExamples;

/// <summary>
/// A sample configuration to show how to derive from BaseConfiguration and use default values and validations.
/// </summary>
public class SampleConfig() : BaseConfiguration<SampleConfig>("Configs/SampleConfig.json")
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