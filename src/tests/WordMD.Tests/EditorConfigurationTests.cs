using FluentAssertions;

[TestFixture]
public class EditorConfigurationTests
{
    [Test]
    public void DetectInstalledEditors_ShouldReturnAtLeastVSCode()
    {
        var editors = EditorConfiguration.DetectInstalledEditors();

        editors.Should().NotBeEmpty();
        editors.Should().Contain("vscode");
    }

    [Test]
    public void GetDefaultEditor_ShouldReturnFirstEditor()
    {
        var editors = EditorConfiguration.DetectInstalledEditors();
        EditorConfiguration.SetEditorOrder(editors);

        var defaultEditor = EditorConfiguration.GetDefaultEditor();

        defaultEditor.Should().Be(editors.First());
    }

    [Test]
    public async Task SetEditorOrder_ShouldPersistToConfig()
    {
        var order = new List<string> {"vscode", "rider", "notepad"};

        EditorConfiguration.SetEditorOrder(order);

        var retrievedOrder = EditorConfiguration.GetEditorOrder();

        await Verify(retrievedOrder);
    }

    [Test]
    public void GetEditor_WithValidName_ShouldReturnEditor()
    {
        var editor = EditorConfiguration.GetEditor("vscode");

        editor.Should().NotBeNull();
        editor.Name.Should().Be("vscode");
        editor.DisplayName.Should().Be("Visual Studio Code");
    }

    [Test]
    public void GetEditor_WithInvalidName_ShouldReturnNull()
    {
        var editor = EditorConfiguration.GetEditor("nonexistent");

        editor.Should().BeNull();
    }
}