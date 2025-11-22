using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using WordMD.Conversion;
using WordMD.Core;

[TestFixture]
public class FileChangeWatcherTests
{
    private string _testDirectory = null!;

    [SetUp]
    public void Setup()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "WordMD.Tests", Guid.NewGuid().ToString());
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
    public async Task FileChangeWatcher_ShouldDetectFileChanges()
    {
        var changeDetected = false;
        using var watcher = new FileChangeWatcher(
            _testDirectory,
            () => changeDetected = true,
            NullLogger<EditorLauncher>.Instance);

        // Create a file
        var testFile = Path.Combine(_testDirectory, "test.md");
        await File.WriteAllTextAsync(testFile, "# Test");

        // Wait for the watcher to detect the change
        await Task.Delay(1000);

        changeDetected.Should().BeTrue();
    }

    [Test]
    public async Task FileChangeWatcher_ShouldDetectFileModifications()
    {
        var testFile = Path.Combine(_testDirectory, "test.md");
        await File.WriteAllTextAsync(testFile, "# Initial");

        var changeCount = 0;
        using var watcher = new FileChangeWatcher(
            _testDirectory,
            () => changeCount++,
            NullLogger<EditorLauncher>.Instance);

        // Wait for initialization
        await Task.Delay(500);
        changeCount = 0; // Reset after initialization

        // Modify the file
        await File.WriteAllTextAsync(testFile, "# Modified");

        // Wait for the watcher to detect the change
        await Task.Delay(1000);

        changeCount.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task FileChangeWatcher_ShouldDebounceRapidChanges()
    {
        var testFile = Path.Combine(_testDirectory, "test.md");
        await File.WriteAllTextAsync(testFile, "# Initial");

        var changeCount = 0;
        using var watcher = new FileChangeWatcher(
            _testDirectory,
            () => changeCount++,
            NullLogger<EditorLauncher>.Instance);

        // Wait for initialization
        await Task.Delay(500);
        changeCount = 0; // Reset after initialization

        // Make multiple rapid changes
        for (var i = 0; i < 5; i++)
        {
            await File.WriteAllTextAsync(testFile, $"# Change {i}");
            await Task.Delay(100); // Less than debounce interval
        }

        // Wait for debounce
        await Task.Delay(1000);

        // Should have fewer callbacks than changes due to debouncing
        changeCount.Should().BeLessThan(5);
    }
}
