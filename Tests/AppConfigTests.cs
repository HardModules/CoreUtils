using System.ComponentModel;
using System.Runtime.Serialization;
using HardDev.CoreUtils.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace HardDev.Tests;

[TestClass]
public class AppConfigTests : IDisposable
{
    private const string CONFIG_FILE_NAME = "test_config.json";
    private const string ANOTHER_TEST_CONFIG = "another_test_config.json";

    [DataContract]
    public class TestConfiguration : BaseConfiguration<TestConfiguration>
    {
        [DataMember, DefaultValue("default string")]
        public string TestString { get; set; }

        [DataMember, DefaultValue(42)]
        public int TestInt { get; set; }

        [DataMember, CollectionDefaultValue(typeof(List<string>), "item1", "item2")]
        public List<string> TestList { get; set; }

        [DataMember, CollectionDefaultValue(typeof(Dictionary<string, int>), "key1", 1, "key2", 2)]
        public Dictionary<string, int> TestDictionary { get; set; }

        [DataMember, CollectionDefaultValue(typeof(int[]), 1, 2, 3)]
        public int[] TestArray { get; set; }

        public TestConfiguration() : base(CONFIG_FILE_NAME)
        {
        }
    }

    [DataContract]
    public class AnotherTestConfiguration : BaseConfiguration<AnotherTestConfiguration>
    {
        [DataMember, DefaultValue("second config")]
        public string AnotherTestString { get; set; }

        [DataMember, DefaultValue(24)]
        public int AnotherTestInt { get; set; }

        public AnotherTestConfiguration() : base(ANOTHER_TEST_CONFIG)
        {
        }
    }

    [TestInitialize]
    public void Cleanup()
    {
        AppConfig.Clear();
        if (File.Exists(CONFIG_FILE_NAME))
        {
            File.Delete(CONFIG_FILE_NAME);
        }
    }

    public void Dispose()
    {
        Cleanup();
    }

    [TestMethod]
    public void Get_DefaultValues_AreSetCorrectly()
    {
        var config = AppConfig.Get<TestConfiguration>();

        Assert.AreEqual("default string", config.TestString);
        Assert.AreEqual(42, config.TestInt);
        CollectionAssert.AreEqual(new List<string> { "item1", "item2" }, config.TestList);
        CollectionAssert.AreEqual(new Dictionary<string, int> { { "key1", 1 }, { "key2", 2 } }, config.TestDictionary);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, config.TestArray);
    }

    [TestMethod]
    public void Save_ConfigValuesAreSavedToFile()
    {
        var config = AppConfig.Get<TestConfiguration>();
        config.TestString = "new string";
        config.TestInt = 99;
        config.TestList.Add("item3");
        config.TestDictionary.Add("key3", 3);
        config.TestArray = new[] { 4, 5, 6 };

        config.Save();

        var configFileContent = File.ReadAllText(CONFIG_FILE_NAME);
        var deserializedConfig = JsonConvert.DeserializeObject<TestConfiguration>(configFileContent);

        Assert.AreEqual(config.TestString, deserializedConfig.TestString);
        Assert.AreEqual(config.TestInt, deserializedConfig.TestInt);
        CollectionAssert.AreEqual(config.TestList, deserializedConfig.TestList);
        CollectionAssert.AreEqual(config.TestDictionary, deserializedConfig.TestDictionary);
        CollectionAssert.AreEqual(config.TestArray, deserializedConfig.TestArray);
    }

    [TestMethod]
    public void Reset_RevertsConfigToDefaultValues()
    {
        var config = AppConfig.Get<TestConfiguration>();

        config.TestString = "new string";
        config.TestInt = 99;
        config.TestList.Add("item3");
        config.TestDictionary.Add("key3", 3);
        config.TestArray = new[] { 4, 5, 6 };

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
        var config = AppConfig.Get<TestConfiguration>();

        config.TestString = "new string";
        config.TestInt = 99;
        config.TestList.Add("item3");
        config.TestDictionary.Add("key3", 3);
        config.TestArray = new[] { 4, 5, 6 };

        config.Save();

        var newConfig = AppConfig.Get<TestConfiguration>();
        newConfig.Load();

        Assert.AreEqual("new string", newConfig.TestString);
        Assert.AreEqual(99, newConfig.TestInt);
        CollectionAssert.AreEqual(new List<string> { "item1", "item2", "item3" }, newConfig.TestList);
        CollectionAssert.AreEqual(new Dictionary<string, int> { { "key1", 1 }, { "key2", 2 }, { "key3", 3 } }, newConfig.TestDictionary);
        CollectionAssert.AreEqual(new[] { 4, 5, 6 }, newConfig.TestArray);
    }

    [TestMethod]
    public void Load_ConfigValuesAreLoadedFromFile()
    {
        const string FILE_CONTENT = @"{
            ""TestString"": ""modified string"",
            ""TestInt"": 888,
            ""TestList"": [""item3"", ""item4""],
            ""TestDictionary"": { ""key3"": 3, ""key4"": 4 },
            ""TestArray"": [4, 5, 6]
        }";
        File.WriteAllText(CONFIG_FILE_NAME, FILE_CONTENT);

        var config = AppConfig.Get<TestConfiguration>();
        config.Load();

        Assert.AreEqual("modified string", config.TestString);
        Assert.AreEqual(888, config.TestInt);
        CollectionAssert.AreEqual(new List<string> { "item3", "item4" }, config.TestList);
        CollectionAssert.AreEqual(new Dictionary<string, int> { { "key3", 3 }, { "key4", 4 } }, config.TestDictionary);
        CollectionAssert.AreEqual(new[] { 4, 5, 6 }, config.TestArray);
    }

    [TestMethod]
    public void Load_BrokenJson_LoadsDefaultValuesAndUpdatesFile()
    {
        const string BROKEN_JSON = @"{ ""TestString"": ""modified string"", ""Test";
        File.WriteAllText(CONFIG_FILE_NAME, BROKEN_JSON);

        var config = AppConfig.Get<TestConfiguration>();

        Assert.AreEqual("default string", config.TestString);
        Assert.AreEqual(42, config.TestInt);
        CollectionAssert.AreEqual(new List<string> { "item1", "item2" }, config.TestList);
        CollectionAssert.AreEqual(new Dictionary<string, int> { { "key1", 1 }, { "key2", 2 } }, config.TestDictionary);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, config.TestArray);

        config.Save();

        var configFileContent = File.ReadAllText(CONFIG_FILE_NAME);
        var deserializedConfig = JsonConvert.DeserializeObject<TestConfiguration>(configFileContent);

        Assert.AreEqual(config.TestString, deserializedConfig.TestString);
        Assert.AreEqual(config.TestInt, deserializedConfig.TestInt);
        CollectionAssert.AreEqual(config.TestList, deserializedConfig.TestList);
        CollectionAssert.AreEqual(config.TestDictionary, deserializedConfig.TestDictionary);
        CollectionAssert.AreEqual(config.TestArray, deserializedConfig.TestArray);
    }

    [TestMethod]
    public void Load_EmptyJson_LoadsDefaultValues()
    {
        File.WriteAllText(CONFIG_FILE_NAME, string.Empty);

        var config = AppConfig.Get<TestConfiguration>();
        config.Load();

        Assert.AreEqual("default string", config.TestString);
        Assert.AreEqual(42, config.TestInt);
        CollectionAssert.AreEqual(new List<string> { "item1", "item2" }, config.TestList);
        CollectionAssert.AreEqual(new Dictionary<string, int> { { "key1", 1 }, { "key2", 2 } }, config.TestDictionary);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, config.TestArray);
    }

    [TestMethod]
    public void Load_NonexistentFile_CreatesNewFile()
    {
        var config = AppConfig.Get<TestConfiguration>();
        config.Load();

        Assert.AreEqual("default string", config.TestString);
        Assert.AreEqual(42, config.TestInt);
        CollectionAssert.AreEqual(new List<string> { "item1", "item2" }, config.TestList);
        CollectionAssert.AreEqual(new Dictionary<string, int> { { "key1", 1 }, { "key2", 2 } }, config.TestDictionary);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, config.TestArray);

        Assert.IsTrue(File.Exists(CONFIG_FILE_NAME));
    }

    [TestMethod]
    public void Get_CanHandleTwoConfigurationTypes()
    {
        var firstConfig = AppConfig.Get<TestConfiguration>();
        var secondConfig = AppConfig.Get<AnotherTestConfiguration>();

        Assert.AreEqual("default string", firstConfig.TestString);
        Assert.AreEqual(42, firstConfig.TestInt);

        Assert.AreEqual("second config", secondConfig.AnotherTestString);
        Assert.AreEqual(24, secondConfig.AnotherTestInt);
    }

    [TestMethod]
    public void ClearConfiguration_RemovesAllCachedConfigurations()
    {
        var firstConfig = AppConfig.Get<TestConfiguration>();

        firstConfig.TestString = "new value";
        Assert.AreEqual("new value", firstConfig.TestString);

        AppConfig.Clear();

        var secondConfig = AppConfig.Get<TestConfiguration>();

        Assert.AreNotSame(firstConfig, secondConfig);
        Assert.AreEqual("default string", secondConfig.TestString);
    }
}