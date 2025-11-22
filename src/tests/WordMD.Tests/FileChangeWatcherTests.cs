using FluentAssertions;

[TestFixture]
public class FileChangeWatcherTests
{
    string testDirectory = null!;

    [SetUp]
    public void Setup()
    {
        testDirectory = Path.Combine(Path.GetTempPath(), "WordMD.Tests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(testDirectory))
        {
            Directory.Delete(testDirectory, true);
        }
    }

    [Test]
    public async Task FileChangeWatcher_ShouldDetectFileChanges()
    {
        var changeDetected = false;
        using var watcher = new FileChangeWatcher(
            testDirectory,
            () => changeDetected = true);

        // Create a file
        var testFile = Path.Combine(testDirectory, "test.md");
        await File.WriteAllTextAsync(testFile, "# Test");

        // Wait for the watcher to detect the change
        await Task.Delay(1000);

        changeDetected.Should().BeTrue();
    }

    [Test]
    public async Task FileChangeWatcher_ShouldDetectFileModifications()
    {
        var testFile = Path.Combine(testDirectory, "test.md");
        await File.WriteAllTextAsync(testFile, "# Initial");

        var changeCount = 0;
        using var watcher = new FileChangeWatcher(
            testDirectory,
            () => changeCount++);

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
        var testFile = Path.Combine(testDirectory, "test.md");
        await File.WriteAllTextAsync(testFile, "# Initial");

        var changeCount = 0;
        using var watcher = new FileChangeWatcher(
            testDirectory,
            () => changeCount++);

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
