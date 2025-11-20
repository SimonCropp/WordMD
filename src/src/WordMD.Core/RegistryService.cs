using Microsoft.Win32;

namespace WordMD.Core;

public class RegistryService
{
    private const string ShellKeyPath = @"*\shell";
    private const string DocxShellKeyPath = @"Word.Document.12\shell";

    public void RegisterContextMenu(List<InstalledEditor> editors, string? defaultEditor)
    {
        var wordMdExePath = GetWordMdExecutablePath();

        // Register main "WordMD Edit" menu item
        RegisterMainMenuItem(wordMdExePath, defaultEditor);

        // Register submenu with all editors
        RegisterSubmenuItems(wordMdExePath, editors);
    }

    private void RegisterMainMenuItem(string wordMdExePath, string? defaultEditor)
    {
        using var key = Registry.ClassesRoot.CreateSubKey($@"{DocxShellKeyPath}\WordMD Edit");
        key.SetValue("", "WordMD Edit");
        key.SetValue("Icon", $"\"{wordMdExePath}\"");

        using var commandKey = key.CreateSubKey("command");
        commandKey.SetValue("", $"\"{wordMdExePath}\" \"%1\"");
    }

    private void RegisterSubmenuItems(string wordMdExePath, List<InstalledEditor> editors)
    {
        // Create main submenu
        using var submenuKey = Registry.ClassesRoot.CreateSubKey($@"{DocxShellKeyPath}\WordMD");
        submenuKey.SetValue("MUIVerb", "WordMD");
        submenuKey.SetValue("SubCommands", "");
        submenuKey.SetValue("Icon", $"\"{wordMdExePath}\"");

        using var shellKey = submenuKey.CreateSubKey("shell");

        // Add each editor as a submenu item
        for (int i = 0; i < editors.Count; i++)
        {
            var editor = editors[i];
            var itemKey = $"item{i}";

            using var editorKey = shellKey.CreateSubKey(itemKey);
            editorKey.SetValue("", editor.Definition.DisplayName);
            editorKey.SetValue("Icon", $"\"{editor.ExecutablePath}\"");

            using var commandKey = editorKey.CreateSubKey("command");
            commandKey.SetValue("", $"\"{wordMdExePath}\" \"%1\" {editor.Definition.Name}");
        }
    }

    public void UnregisterContextMenu()
    {
        try
        {
            Registry.ClassesRoot.DeleteSubKeyTree($@"{DocxShellKeyPath}\WordMD Edit", false);
        }
        catch { }

        try
        {
            Registry.ClassesRoot.DeleteSubKeyTree($@"{DocxShellKeyPath}\WordMD", false);
        }
        catch { }
    }

    private static string GetWordMdExecutablePath()
    {
        // When running as a dotnet tool, we need to return the path to the tool
        // This is typically in %USERPROFILE%\.dotnet\tools\wordmd.exe
        var toolsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".dotnet", "tools", "wordmd.exe");

        if (File.Exists(toolsPath))
        {
            return toolsPath;
        }

        // Fallback to current executable
        return Environment.ProcessPath ?? "wordmd";
    }
}
