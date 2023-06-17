using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using HardDev.CoreUtils.Config;

namespace ConfigExamples;

/// <summary>
/// A sample configuration to show how to derive from BaseConfiguration and use default values and validations.
/// </summary>
[DataContract]
public class SampleConfig : BaseConfiguration<SampleConfig>
{
    /// <summary>
    /// Gets or sets an integer value.
    /// </summary>
    [DataMember, DefaultValue(42)]
    public int IntegerValue { get; set; }

    /// <summary>
    /// Gets or sets a double value.
    /// </summary>
    [DataMember, DefaultValue(3.14)]
    public double DoubleValue { get; set; }

    /// <summary>
    /// Gets or sets a float value.
    /// </summary>
    [DataMember, DefaultValue(123.456f)]
    public float FloatValue { get; set; }

    /// <summary>
    /// Gets or sets a boolean value.
    /// </summary>
    [DataMember, DefaultValue(true)]
    public bool BooleanValue { get; set; }

    /// <summary>
    /// Gets or sets a URL as a string.
    /// </summary>
    [DataMember, DefaultValue("http://default_url.com"), Url(ErrorMessage = "Invalid URL format")]
    public string Url { get; set; }

    /// <summary>
    /// Gets or sets a read-only list of strings representing accounts.
    /// </summary>
    [DataMember, CollectionDefaultValue(typeof(string[]), "Value1", "Value2"), Required(ErrorMessage = "Accounts cannot be null"),
     MinLength(1, ErrorMessage = "Accounts cannot be empty")]
    public IReadOnlyList<string> Accounts { get; set; }

    /// <summary>
    /// Gets or sets a read-only list of integers representing numbers.
    /// </summary>
    [DataMember, CollectionDefaultValue(typeof(List<int>), 1, 2, 3), Required(ErrorMessage = "Numbers cannot be null"),
     MinLength(1, ErrorMessage = "Numbers cannot be empty")]
    public IReadOnlyList<int> Numbers { get; set; }

    /// <summary>
    /// Gets or sets a dictionary with string keys and integer values.
    /// </summary>
    [DataMember, CollectionDefaultValue(typeof(Dictionary<string, int>), "Key1", 1, "Key2", 2),
     Required(ErrorMessage = "Example dictionary cannot be null"), MinLength(1, ErrorMessage = "ExampleDictionary cannot be empty")]
    public IDictionary<string, int> ExampleDictionary { get; set; }

    public SampleConfig() : base("Configs/SampleConfig.json")
    {
    }
}