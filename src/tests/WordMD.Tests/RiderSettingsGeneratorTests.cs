using WordMD.Core;

namespace WordMD.Tests;

[TestFixture]
public class RiderSettingsGeneratorTests
{
    private RiderSettingsGenerator _generator = null!;
    private string _testDirectory = null!;

    [SetUp]
    public void Setup()
    {
        _generator = new RiderSettingsGenerator();
        _testDirectory = Path.Combine(Path.GetTempPath(), $"wordmd_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    [Test]
    public void GenerateDotSettings_ShouldCreateFile()
    {
        _generator.GenerateDotSettings(_testDirectory);

        var dotSettingsPath = Path.Combine(_testDirectory, "Default.DotSettings");

        Assert.That(File.Exists(dotSettingsPath), Is.True);
    }

    [Test]
    public async Task GenerateDotSettings_ShouldContainAutoSaveDisabled()
    {
        _generator.GenerateDotSettings(_testDirectory);

        var dotSettingsPath = Path.Combine(_testDirectory, "Default.DotSettings");
        var content = await File.ReadAllTextAsync(dotSettingsPath);

        await Verify(content);
    }

    [Test]
    public void GenerateDotSettings_Content_ShouldBeValidXml()
    {
        _generator.GenerateDotSettings(_testDirectory);

        var dotSettingsPath = Path.Combine(_testDirectory, "Default.DotSettings");
        var content = File.ReadAllText(dotSettingsPath);

        Assert.DoesNotThrow(() =>
        {
            var xml = System.Xml.Linq.XDocument.Parse(content);
            Assert.That(xml.Root, Is.Not.Null);
        });
    }

    [Test]
    public void GenerateDotSettings_ShouldContainAutoSaveFalse()
    {
        _generator.GenerateDotSettings(_testDirectory);

        var dotSettingsPath = Path.Combine(_testDirectory, "Default.DotSettings");
        var content = File.ReadAllText(dotSettingsPath);

        Assert.Multiple(() =>
        {
            Assert.That(content, Does.Contain("AutoSave"));
            Assert.That(content, Does.Contain("False"));
        });
    }

    [Test]
    public void GenerateDotSettings_CalledMultipleTimes_ShouldOverwrite()
    {
        _generator.GenerateDotSettings(_testDirectory);
        var firstContent = File.ReadAllText(Path.Combine(_testDirectory, "Default.DotSettings"));

        _generator.GenerateDotSettings(_testDirectory);
        var secondContent = File.ReadAllText(Path.Combine(_testDirectory, "Default.DotSettings"));

        Assert.That(secondContent, Is.EqualTo(firstContent));
    }

    [Test]
    public void GenerateDotSettings_InNonExistentDirectory_ShouldThrow()
    {
        var nonExistentDir = Path.Combine(_testDirectory, "nonexistent");

        Assert.Throws<DirectoryNotFoundException>(() =>
        {
            _generator.GenerateDotSettings(nonExistentDir);
        });
    }
}
