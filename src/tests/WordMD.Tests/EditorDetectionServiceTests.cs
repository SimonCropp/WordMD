using WordMD.Core;

namespace WordMD.Tests;

[TestFixture]
public class EditorDetectionServiceTests
{
    private EditorDetectionService _service = null!;

    [SetUp]
    public void Setup()
    {
        _service = new EditorDetectionService();
    }

    [Test]
    public void DetectInstalledEditors_ShouldReturnList()
    {
        var editors = _service.DetectInstalledEditors();

        Assert.That(editors, Is.Not.Null);
    }

    [Test]
    public async Task DetectInstalledEditors_ShouldReturnValidStructure()
    {
        var editors = _service.DetectInstalledEditors();

        var result = new
        {
            EditorCount = editors.Count,
            Editors = editors.Select(e => new
            {
                e.Definition.Name,
                e.Definition.DisplayName,
                HasExecutablePath = !string.IsNullOrEmpty(e.ExecutablePath)
            }).ToList()
        };

        await Verify(result)
            .IgnoreMembers("EditorCount", "Editors[*].HasExecutablePath")
            .DontScrubGuids();
    }

    [Test]
    public void DetectInstalledEditors_AllResults_ShouldHaveValidPaths()
    {
        var editors = _service.DetectInstalledEditors();

        foreach (var editor in editors)
        {
            Assert.Multiple(() =>
            {
                Assert.That(editor.ExecutablePath, Is.Not.Null.And.Not.Empty,
                    $"Editor {editor.Definition.Name} has null/empty path");
                Assert.That(File.Exists(editor.ExecutablePath), Is.True,
                    $"Editor {editor.Definition.Name} path does not exist: {editor.ExecutablePath}");
            });
        }
    }

    [Test]
    public void DetectInstalledEditors_ShouldNotReturnDuplicates()
    {
        var editors = _service.DetectInstalledEditors();
        var editorNames = editors.Select(e => e.Definition.Name).ToList();
        var uniqueNames = editorNames.Distinct().ToList();

        Assert.That(editorNames, Has.Count.EqualTo(uniqueNames.Count));
    }

    [Test]
    public void DetectInstalledEditors_ResultsHaveDefinitions()
    {
        var editors = _service.DetectInstalledEditors();

        foreach (var editor in editors)
        {
            Assert.That(editor.Definition, Is.Not.Null);
        }
    }

    [Test]
    public void InstalledEditor_ShouldBeRecord()
    {
        // Verify that InstalledEditor behaves as a record
        var definition = KnownEditors.AllEditors.First();
        var editor1 = new InstalledEditor(definition, @"C:\test\editor.exe");
        var editor2 = new InstalledEditor(definition, @"C:\test\editor.exe");
        var editor3 = new InstalledEditor(definition, @"C:\other\editor.exe");

        Assert.Multiple(() =>
        {
            Assert.That(editor1, Is.EqualTo(editor2));
            Assert.That(editor1, Is.Not.EqualTo(editor3));
            Assert.That(editor1.GetHashCode(), Is.EqualTo(editor2.GetHashCode()));
        });
    }

    [Test]
    public void DetectInstalledEditors_CalledMultipleTimes_ShouldBeConsistent()
    {
        var firstDetection = _service.DetectInstalledEditors();
        var secondDetection = _service.DetectInstalledEditors();

        Assert.That(firstDetection.Count, Is.EqualTo(secondDetection.Count));

        for (int i = 0; i < firstDetection.Count; i++)
        {
            Assert.Multiple(() =>
            {
                Assert.That(firstDetection[i].Definition.Name, 
                    Is.EqualTo(secondDetection[i].Definition.Name));
                Assert.That(firstDetection[i].ExecutablePath, 
                    Is.EqualTo(secondDetection[i].ExecutablePath));
            });
        }
    }
}
