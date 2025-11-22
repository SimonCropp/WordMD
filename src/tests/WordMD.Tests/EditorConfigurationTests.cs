
[TestFixture]
public class EditorConfigurationTests
{
    [Test]
    public void DetectInstalledEditors_ShouldReturnAtLeastVSCode()
    {
        var editors = EditorConfiguration.DetectInstalledEditors();

        IsNotEmpty(editors);
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
    public Task GetEditor_WithValidName_ShouldReturnEditor()
    {
        var editor = EditorConfiguration.GetEditor("vscode");

        return Verify(editor);
    }

    [Test]
    public void GetEditor_WithInvalidName_ShouldReturnNull()
    {
        var editor = EditorConfiguration.GetEditor("nonexistent");

        IsNull(editor);
    }
}