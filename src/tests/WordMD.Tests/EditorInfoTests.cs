using FluentAssertions;
using WordMD.Editors;

[TestFixture]
public class EditorInfoTests
{
    [Test]
    public void AllEditors_ShouldContainExpectedEditors()
    {
        var editors = EditorInfo.AllEditors;

        editors.Should().NotBeEmpty();
        editors.Should().Contain(e => e.Name == "vscode");
        editors.Should().Contain(e => e.Name == "rider");
        editors.Should().Contain(e => e.Name == "notepad");
        editors.Should().Contain(e => e.Name == "typora");
    }

    [Test]
    public async Task VSCode_Properties_ShouldBeCorrect()
    {
        var vscode = EditorInfo.VSCode;

        var properties = new
        {
            vscode.Name,
            vscode.DisplayName,
            vscode.ExecutableName,
            vscode.CommandLineArgs
        };

        await Verify(properties);
    }

    [Test]
    public async Task Rider_Properties_ShouldBeCorrect()
    {
        var rider = EditorInfo.Rider;

        var properties = new
        {
            rider.Name,
            rider.DisplayName,
            rider.ExecutableName,
            rider.CommandLineArgs
        };

        await Verify(properties);
    }

    [Test]
    public void GetExecutablePath_WithVSCode_ShouldReturnPath()
    {
        var vscode = EditorInfo.VSCode;

        if (vscode.IsInstalled())
        {
            var path = vscode.GetExecutablePath();

            path.Should().NotBeNull();
            File.Exists(path).Should().BeTrue();
        }
    }
}