using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using WordMD.Editors;

[TestFixture]
public class EditorConfigurationTests
{
    private EditorConfiguration _config = null!;

    [SetUp]
    public void Setup()
    {
        _config = new EditorConfiguration(NullLogger<EditorConfiguration>.Instance);
    }

    [Test]
    public void DetectInstalledEditors_ShouldReturnAtLeastVSCode()
    {
        var editors = _config.DetectInstalledEditors();
        
        editors.Should().NotBeEmpty();
        editors.Should().Contain("vscode");
    }

    [Test]
    public void GetDefaultEditor_ShouldReturnFirstEditor()
    {
        var editors = _config.DetectInstalledEditors();
        _config.SetEditorOrder(editors);
        
        var defaultEditor = _config.GetDefaultEditor();
        
        defaultEditor.Should().Be(editors.First());
    }

    [Test]
    public async Task SetEditorOrder_ShouldPersistToConfig()
    {
        var order = new List<string> { "vscode", "rider", "notepad" };
        
        _config.SetEditorOrder(order);
        
        var retrievedOrder = _config.GetEditorOrder();
        
        await Verify(retrievedOrder);
    }

    [Test]
    public void GetEditor_WithValidName_ShouldReturnEditor()
    {
        var editor = _config.GetEditor("vscode");
        
        editor.Should().NotBeNull();
        editor!.Name.Should().Be("vscode");
        editor.DisplayName.Should().Be("Visual Studio Code");
    }

    [Test]
    public void GetEditor_WithInvalidName_ShouldReturnNull()
    {
        var editor = _config.GetEditor("nonexistent");
        
        editor.Should().BeNull();
    }
}
