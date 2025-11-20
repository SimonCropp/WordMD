using WordMD.Core;

namespace WordMD.Tests;

[TestFixture]
public class MarkdownFileWatcherTests
{
    private string _testDirectory = null!;

    [SetUp]
    public void Setup()
    {
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
    public async Task FileWatcher_WhenFileCreated_ShouldTriggerCallback()
    {
        var callbackTriggered = false;
        using var watcher = new MarkdownFileWatcher(_testDirectory, () => callbackTriggered = true);

        // Give the watcher time to initialize
        await Task.Delay(100);

        var testFile = Path.Combine(_testDirectory, "test.md");
        File.WriteAllText(testFile, "# Test");

        // Wait for file system event
        await Task.Delay(1000);

        Assert.That(callbackTriggered, Is.True);
    }

    [Test]
    public async Task FileWatcher_WhenFileModified_ShouldTriggerCallback()
    {
        var testFile = Path.Combine(_testDirectory, "test.md");
        File.WriteAllText(testFile, "# Initial");

        var callbackCount = 0;
        using var watcher = new MarkdownFileWatcher(_testDirectory, () => callbackCount++);

        await Task.Delay(100);

        File.WriteAllText(testFile, "# Modified");

        await Task.Delay(1000);

        Assert.That(callbackCount, Is.GreaterThan(0));
    }

    [Test]
    public async Task FileWatcher_WhenHiddenFileModified_ShouldNotTriggerCallback()
    {
        var callbackTriggered = false;
        using var watcher = new MarkdownFileWatcher(_testDirectory, () => callbackTriggered = true);

        await Task.Delay(100);

        var hiddenFile = Path.Combine(_testDirectory, ".hidden");
        File.WriteAllText(hiddenFile, "content");

        await Task.Delay(1000);

        Assert.That(callbackTriggered, Is.False);
    }

    [Test]
    public async Task FileWatcher_WhenTempFileModified_ShouldNotTriggerCallback()
    {
        var callbackTriggered = false;
        using var watcher = new MarkdownFileWatcher(_testDirectory, () => callbackTriggered = true);

        await Task.Delay(100);

        var tempFile = Path.Combine(_testDirectory, "temp.tmp");
        File.WriteAllText(tempFile, "content");

        await Task.Delay(1000);

        Assert.That(callbackTriggered, Is.False);
    }

    [Test]
    public async Task FileWatcher_WhenImageAdded_ShouldTriggerCallback()
    {
        var imagesDir = Path.Combine(_testDirectory, "images");
        Directory.CreateDirectory(imagesDir);

        var callbackTriggered = false;
        using var watcher = new MarkdownFileWatcher(_testDirectory, () => callbackTriggered = true);

        await Task.Delay(100);

        var imageFile = Path.Combine(imagesDir, "test.png");
        File.WriteAllBytes(imageFile, [0x89, 0x50, 0x4E, 0x47]);

        await Task.Delay(1000);

        Assert.That(callbackTriggered, Is.True);
    }

    [Test]
    public async Task FileWatcher_MultipleRapidChanges_ShouldDebounce()
    {
        var testFile = Path.Combine(_testDirectory, "test.md");
        File.WriteAllText(testFile, "initial");

        var callbackCount = 0;
        using var watcher = new MarkdownFileWatcher(_testDirectory, () => callbackCount++);

        await Task.Delay(100);

        // Make multiple rapid changes
        for (int i = 0; i < 5; i++)
        {
            File.WriteAllText(testFile, $"content {i}");
            await Task.Delay(50); // Less than debounce interval
        }

        // Wait for debounce to settle
        await Task.Delay(1000);

        // Should have fewer callbacks than changes due to debouncing
        Assert.That(callbackCount, Is.LessThan(5));
    }

    [Test]
    public void FileWatcher_Dispose_ShouldStopWatching()
    {
        var callbackTriggered = false;
        var watcher = new MarkdownFileWatcher(_testDirectory, () => callbackTriggered = true);

        watcher.Dispose();

        var testFile = Path.Combine(_testDirectory, "test.md");
        File.WriteAllText(testFile, "test");

        Thread.Sleep(1000);

        Assert.That(callbackTriggered, Is.False);
    }
}
