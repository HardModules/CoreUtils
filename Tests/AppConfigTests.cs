using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using HardDev.CoreUtils.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HardDev.Tests;

[TestClass]
public class AppConfigTests : IDisposable
{
    private const string ConfigFileName = "test_config.json";
    private const string AnotherTestConfig = "another_test_config.json";

    private const string NormJsonContent = """
                                           {
                                              "TestString": "modified string",
                                              "TestInt": 888,
                                              "TestList": ["item3", "item4"],
                                              "TestDictionary": { "key3": 3, "key4": 4 },
                                              "TestArray": [4, 5, 6]
                                           }
                                           """;

    private const string BrokenJsonContent = """@ "TestString": "modified string, "Test" """;


    public class TestConfiguration() : BaseConfiguration<TestConfiguration>(ConfigFileName)
    {
        public List<string> TestList { get; set; } = ["item1", "item2"];
        public string TestString { get; set; } = "default string";

        [Range(1, 100)]
        public int TestInt { get; set; } = 42;

        public Dictionary<string, int> TestDictionary { get; set; } = new() { ["key1"] = 1, ["key2"] = 2 };
        public int[] TestArray { get; set; } = [1, 2, 3];
    }

    public class AnotherTestConfiguration() : BaseConfiguration<AnotherTestConfiguration>(AnotherTestConfig)
    {
        public string AnotherTestString { get; set; } = "second config";
        public int AnotherTestInt { get; set; } = 24;
    }

    [TestInitialize]
    public void Cleanup()
    {
        AppConfig.Clear();

        if (File.Exists(ConfigFileName))
        {
            File.Delete(ConfigFileName);
        }

        if (File.Exists(AnotherTestConfig))
        {
            File.Delete(AnotherTestConfig);
        }
    }

    public void Dispose()
    {
        Cleanup();
        GC.SuppressFinalize(this);
    }

    [TestMethod]
    public void Get_DefaultValues_AreSetCorrectly()
    {
        TestConfiguration config = AppConfig.Get<TestConfiguration>();

        Assert.AreEqual("default string", config.TestString);
        Assert.AreEqual(42, config.TestInt);
        CollectionAssert.AreEqual(new List<string> { "item1", "item2" }, config.TestList);
        CollectionAssert.AreEqual(new Dictionary<string, int> { { "key1", 1 }, { "key2", 2 } }, config.TestDictionary);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, config.TestArray);
    }

    [TestMethod]
    public void Save_ConfigValuesAreSavedToFile()
    {
        TestConfiguration config = AppConfig.Get<TestConfiguration>();
        config.TestString = "new string";
        config.TestInt = 99;
        config.TestList.Add("item3");
        config.TestDictionary.Add("key3", 3);
        config.TestArray = [4, 5, 6];

        config.Save();

        TestConfiguration testConfiguration = AppConfig.Get<TestConfiguration>();

        Assert.AreEqual(config.TestString, testConfiguration.TestString);
        Assert.AreEqual(config.TestInt, testConfiguration.TestInt);
        CollectionAssert.AreEqual(config.TestList, testConfiguration.TestList);
        CollectionAssert.AreEqual(config.TestDictionary, testConfiguration.TestDictionary);
        CollectionAssert.AreEqual(config.TestArray, testConfiguration.TestArray);
    }

    [TestMethod]
    public void Reset_RevertsConfigToDefaultValues()
    {
        TestConfiguration config = AppConfig.Get<TestConfiguration>();

        config.TestString = "new string";
        config.TestInt = 99;
        config.TestList.Add("item3");
        config.TestDictionary.Add("key3", 3);
        config.TestArray = [4, 5, 6];

        config.Reset();

        Assert.AreEqual("default string", config.TestString);
        Assert.AreEqual(42, config.TestInt);
        CollectionAssert.AreEqual(new List<string> { "item1", "item2" }, config.TestList);
        CollectionAssert.AreEqual(new Dictionary<string, int> { { "key1", 1 }, { "key2", 2 } }, config.TestDictionary);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, config.TestArray);
    }

    [TestMethod]
    public void Save_NewConfigValuesAreReturnedViaGet()
    {
        TestConfiguration config = AppConfig.Get<TestConfiguration>();

        config.TestString = "new string";
        config.TestInt = 99;
        config.TestList.Add("item3");
        config.TestDictionary.Add("key3", 3);
        config.TestArray = [4, 5, 6];

        config.Save();

        AppConfig.Clear();
        TestConfiguration newConfig = AppConfig.Get<TestConfiguration>();
        newConfig.Load();

        Assert.AreEqual("new string", newConfig.TestString);
        Assert.AreEqual(99, newConfig.TestInt);
        CollectionAssert.AreEqual(new List<string> { "item1", "item2", "item3" }, newConfig.TestList);
        CollectionAssert.AreEqual(new Dictionary<string, int> { { "key1", 1 }, { "key2", 2 }, { "key3", 3 } },
            newConfig.TestDictionary);
        CollectionAssert.AreEqual(new[] { 4, 5, 6 }, newConfig.TestArray);
    }

    [TestMethod]
    public void Load_ConfigValuesAreLoadedFromFile()
    {
        File.WriteAllText(ConfigFileName, NormJsonContent);

        TestConfiguration config = AppConfig.GetOrLoad<TestConfiguration>(out bool loaded);

        Assert.IsTrue(loaded);

        Assert.AreEqual("modified string", config.TestString);
        Assert.AreEqual(42, config.TestInt);
        CollectionAssert.AreEqual(new List<string> { "item3", "item4" }, config.TestList);
        CollectionAssert.AreEqual(new Dictionary<string, int> { { "key3", 3 }, { "key4", 4 } }, config.TestDictionary);
        CollectionAssert.AreEqual(new[] { 4, 5, 6 }, config.TestArray);

        AppConfig.Clear();

        File.WriteAllText(ConfigFileName, BrokenJsonContent);

        AppConfig.GetOrLoad<TestConfiguration>(out loaded);

        Assert.AreEqual(false, loaded);
    }

    [TestMethod]
    public void SaveAndLoad_ConfigValues()
    {
        TestConfiguration config = AppConfig.GetOrLoad<TestConfiguration>();
        config.TestString = "modified string";
        config.Save();

        AppConfig.Clear();

        config = AppConfig.GetOrLoad<TestConfiguration>();

        Assert.AreEqual("modified string", config.TestString);
    }

    [TestMethod]
    public void Load_InvalidJson_UsesDefaultsAndUpdatesFile()
    {
        File.WriteAllText(ConfigFileName, BrokenJsonContent);

        TestConfiguration config = AppConfig.Get<TestConfiguration>();

        Assert.AreEqual("default string", config.TestString);
        Assert.AreEqual(42, config.TestInt);
        CollectionAssert.AreEqual(new List<string> { "item1", "item2" }, config.TestList);
        CollectionAssert.AreEqual(new Dictionary<string, int> { { "key1", 1 }, { "key2", 2 } }, config.TestDictionary);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, config.TestArray);

        config.Save();

        string configFileContent = File.ReadAllText(ConfigFileName);
        TestConfiguration deserializedConfig = JsonSerializer.Deserialize<TestConfiguration>(configFileContent);

        Assert.AreEqual(config.TestString, deserializedConfig.TestString);
        Assert.AreEqual(config.TestInt, deserializedConfig.TestInt);
        CollectionAssert.AreEqual(config.TestList, deserializedConfig.TestList);
        CollectionAssert.AreEqual(config.TestDictionary, deserializedConfig.TestDictionary);
        CollectionAssert.AreEqual(config.TestArray, deserializedConfig.TestArray);
    }

    [TestMethod]
    public void Load_EmptyJson_LoadsDefaultValues()
    {
        File.WriteAllText(ConfigFileName, string.Empty);

        TestConfiguration config = AppConfig.Get<TestConfiguration>();
        config.Load();

        Assert.AreEqual("default string", config.TestString);
        Assert.AreEqual(42, config.TestInt);
        CollectionAssert.AreEqual(new List<string> { "item1", "item2" }, config.TestList);
        CollectionAssert.AreEqual(new Dictionary<string, int> { { "key1", 1 }, { "key2", 2 } }, config.TestDictionary);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, config.TestArray);
    }

    [TestMethod]
    public void Load_DefaultAndSaveIfFileMissing()
    {
        TestConfiguration config = AppConfig.Get<TestConfiguration>();

        Assert.AreEqual("default string", config.TestString);
        Assert.AreEqual(42, config.TestInt);
        CollectionAssert.AreEqual(new List<string> { "item1", "item2" }, config.TestList);
        CollectionAssert.AreEqual(new Dictionary<string, int> { { "key1", 1 }, { "key2", 2 } }, config.TestDictionary);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, config.TestArray);

        config.Save();

        Assert.IsTrue(File.Exists(ConfigFileName));
    }

    [TestMethod]
    public void Get_CanHandleTwoConfigurationTypes()
    {
        TestConfiguration firstConfig = AppConfig.Get<TestConfiguration>();
        AnotherTestConfiguration secondConfig = AppConfig.Get<AnotherTestConfiguration>();

        Assert.AreEqual("default string", firstConfig.TestString);
        Assert.AreEqual(42, firstConfig.TestInt);

        Assert.AreEqual("second config", secondConfig.AnotherTestString);
        Assert.AreEqual(24, secondConfig.AnotherTestInt);
    }

    [TestMethod]
    public void ClearConfiguration_RemovesAllCachedConfigurations()
    {
        TestConfiguration firstConfig = AppConfig.Get<TestConfiguration>();

        firstConfig.TestString = "new value";

        AppConfig.Clear();

        TestConfiguration secondConfig = AppConfig.Get<TestConfiguration>();

        Assert.AreNotSame(firstConfig, secondConfig);
        Assert.AreEqual("default string", secondConfig.TestString);
    }

    [TestMethod]
    public void ValidateProperties_InvalidValue_SetsDefaultValue()
    {
        var config = new TestConfiguration { TestInt = -1 };

        config.Load();

        Assert.AreEqual(42, config.TestInt);
    }

    [TestMethod]
    public void EnsureValidProperties_InvalidValue_CorrectsValueAndIndicatesChange()
    {
        var config = new TestConfiguration { TestInt = -1 };

        bool result = config.EnsureValidProperties();

        Assert.AreEqual(42, config.TestInt);
        Assert.IsTrue(result);
    }
}
