using WordMD.Core;

namespace WordMD.Tests;

[TestFixture]
public class EditorDefinitionsTests
{
    [Test]
    public async Task AllEditors_ShouldContainExpectedEditors()
    {
        var editorNames = KnownEditors.AllEditors
            .Select(e => new
            {
                e.Name,
                e.DisplayName,
                e.ExecutableName
            })
            .ToList();

        await Verify(editorNames);
    }

    [Test]
    public void AllEditors_ShouldNotBeEmpty()
    {
        Assert.That(KnownEditors.AllEditors, Is.Not.Empty);
    }

    [Test]
    public void AllEditors_ShouldHaveUniqueNames()
    {
        var names = KnownEditors.AllEditors.Select(e => e.Name).ToList();
        var uniqueNames = names.Distinct().ToList();

        Assert.That(names, Has.Count.EqualTo(uniqueNames.Count));
    }

    [Test]
    public async Task GetByName_WithValidName_ShouldReturnEditor()
    {
        var editor = KnownEditors.GetByName("vscode");

        Assert.That(editor, Is.Not.Null);
        await Verify(new
        {
            editor!.Name,
            editor.DisplayName,
            editor.ExecutableName
        });
    }

    [Test]
    public void GetByName_WithInvalidName_ShouldReturnNull()
    {
        var editor = KnownEditors.GetByName("nonexistent");

        Assert.That(editor, Is.Null);
    }

    [Test]
    public void GetByName_ShouldBeCaseInsensitive()
    {
        var lowercase = KnownEditors.GetByName("vscode");
        var uppercase = KnownEditors.GetByName("VSCODE");
        var mixedcase = KnownEditors.GetByName("VsCode");

        Assert.Multiple(() =>
        {
            Assert.That(lowercase, Is.Not.Null);
            Assert.That(uppercase, Is.Not.Null);
            Assert.That(mixedcase, Is.Not.Null);
            Assert.That(lowercase, Is.SameAs(uppercase));
            Assert.That(lowercase, Is.SameAs(mixedcase));
        });
    }

    [Test]
    [TestCase("vscode")]
    [TestCase("rider")]
    [TestCase("typora")]
    [TestCase("markdownmonster")]
    [TestCase("obsidian")]
    [TestCase("notepad")]
    public void GetByName_WithKnownEditor_ShouldReturnEditor(string editorName)
    {
        var editor = KnownEditors.GetByName(editorName);

        Assert.That(editor, Is.Not.Null);
        Assert.That(editor!.Name, Is.EqualTo(editorName).IgnoreCase);
    }

    [Test]
    public void EditorDefinition_ShouldHaveValidProperties()
    {
        foreach (var editor in KnownEditors.AllEditors)
        {
            Assert.Multiple(() =>
            {
                Assert.That(editor.Name, Is.Not.Null.And.Not.Empty, 
                    $"Editor {editor.DisplayName} has null/empty Name");
                Assert.That(editor.DisplayName, Is.Not.Null.And.Not.Empty,
                    $"Editor {editor.Name} has null/empty DisplayName");
                Assert.That(editor.ExecutableName, Is.Not.Null.And.Not.Empty,
                    $"Editor {editor.Name} has null/empty ExecutableName");
                Assert.That(editor.PossiblePaths, Is.Not.Null.And.Not.Empty,
                    $"Editor {editor.Name} has null/empty PossiblePaths");
                Assert.That(editor.ArgumentsPattern, Is.Not.Null.And.Not.Empty,
                    $"Editor {editor.Name} has null/empty ArgumentsPattern");
            });
        }
    }
}
