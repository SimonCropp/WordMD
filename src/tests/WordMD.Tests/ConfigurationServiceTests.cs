using System.Text.Json;
using WordMD.Core;

namespace WordMD.Tests;

[TestFixture]
public class ConfigurationServiceTests
{
    private string _testConfigDirectory = null!;
    private string _testConfigFilePath = null!;

    [SetUp]
    public void Setup()
    {
        _testConfigDirectory = Path.Combine(Path.GetTempPath(), $"wordmd_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testConfigDirectory);
        _testConfigFilePath = Path.Combine(_testConfigDirectory, "config.json");
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testConfigDirectory))
        {
            Directory.Delete(_testConfigDirectory, true);
        }
    }

    private ConfigurationService CreateService()
    {
        // Note: In real implementation, we'd need to inject the config path
        // For now, this is a simplified version
        return new ConfigurationService();
    }

    [Test]
    public async Task NewConfiguration_ShouldHaveDefaultValues()
    {
        var config = new WordMdConfiguration();

        await Verify(config);
    }

    [Test]
    public void NewConfiguration_ShouldHaveEmptyEditorOrder()
    {
        var config = new WordMdConfiguration();

        Assert.That(config.EditorOrder, Is.Empty);
    }

    [Test]
    public void NewConfiguration_ShouldHaveNullDefaultEditor()
    {
        var config = new WordMdConfiguration();

        Assert.That(config.DefaultEditor, Is.Null);
    }

    [Test]
    public async Task Configuration_WithEditorOrder_ShouldSerialize()
    {
        var config = new WordMdConfiguration
        {
            EditorOrder = ["vscode", "rider", "typora"],
            DefaultEditor = "vscode"
        };

        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await Verify(json);
    }

    [Test]
    public void Configuration_ShouldRoundTripThroughJson()
    {
        var original = new WordMdConfiguration
        {
            EditorOrder = ["vscode", "rider", "typora"],
            DefaultEditor = "vscode"
        };

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<WordMdConfiguration>(json);

        Assert.Multiple(() =>
        {
            Assert.That(deserialized, Is.Not.Null);
            Assert.That(deserialized!.EditorOrder, Is.EqualTo(original.EditorOrder));
            Assert.That(deserialized.DefaultEditor, Is.EqualTo(original.DefaultEditor));
        });
    }

    [Test]
    public void UpdateEditorOrder_ShouldModifyConfiguration()
    {
        var config = new WordMdConfiguration();
        var newOrder = new List<string> { "rider", "vscode", "notepad" };

        config.EditorOrder = newOrder;

        Assert.That(config.EditorOrder, Is.EqualTo(newOrder));
    }

    [Test]
    public async Task Configuration_WithComplexOrder_ShouldVerify()
    {
        var config = new WordMdConfiguration
        {
            EditorOrder = 
            [
                "rider",
                "vscode",
                "typora",
                "markdownmonster",
                "obsidian",
                "notepad"
            ],
            DefaultEditor = "rider"
        };

        await Verify(config);
    }
}
