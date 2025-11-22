public static class RegistryManager
{
    const string ContextMenuPath = @"SOFTWARE\Classes\.docx\shell";

    public static void RegisterContextMenu(List<string> editorOrder)
    {
        Log.Information("Registering context menu for WordMD");

        // Remove existing entries first
        RemoveContextMenu();

        // Get the path to wordmd.exe
        var wordmdPath = GetWordMDPath();

        // Register "WordMD Edit" (default editor)
        RegisterDefaultEdit(wordmdPath);

        // Register "WordMD" with submenu
        RegisterSubmenu(wordmdPath, editorOrder);

        Log.Information("Context menu registered successfully");
    }

    static void RemoveContextMenu()
    {
        Log.Information("Removing WordMD context menu entries");

        using var key = Registry.CurrentUser.OpenSubKey(ContextMenuPath, true);
        if (key == null)
        {
            return;
        }

        // Remove WordMD Edit
        key.DeleteSubKeyTree("WordMD Edit", false);

        // Remove WordMD submenu
        key.DeleteSubKeyTree("WordMD", false);

        Log.Information("Context menu entries removed");
    }

    static void RegisterDefaultEdit(string wordmdPath)
    {
        using var key = Registry.CurrentUser.CreateSubKey($@"{ContextMenuPath}\WordMD Edit");
        key.SetValue("", "Edit with WordMD");
        key.SetValue("Icon", wordmdPath);

        using var commandKey = key.CreateSubKey("command");
        commandKey.SetValue("", $"\"{wordmdPath}\" \"%1\"");
    }

    static void RegisterSubmenu(string wordmdPath, List<string> editorOrder)
    {
        using var key = Registry.CurrentUser.CreateSubKey($@"{ContextMenuPath}\WordMD");
        key.SetValue("", "Edit with WordMD...");
        key.SetValue("SubCommands", "");
        key.SetValue("MUIVerb", "Edit with WordMD");
        key.SetValue("Icon", wordmdPath);

        using var shellKey = key.CreateSubKey("shell");

        foreach (var editorName in editorOrder)
        {
            var editor = EditorInfo.AllEditors.FirstOrDefault(e =>
                e.Name.Equals(editorName, StringComparison.OrdinalIgnoreCase));

            if (editor == null)
            {
                continue;
            }

            using var editorKey = shellKey.CreateSubKey(editorName);
            editorKey.SetValue("", editor.DisplayName);

            using var commandKey = editorKey.CreateSubKey("command");
            commandKey.SetValue("", $"\"{wordmdPath}\" \"%1\" {editor.Name}");
        }
    }

    static string GetWordMDPath()
    {
        // Try to find wordmd in PATH
        var paths = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ?? [];

        foreach (var path in paths)
        {
            try
            {
                var wordmdPath = Path.Combine(path, "wordmd.exe");
                if (File.Exists(wordmdPath))
                {
                    return wordmdPath;
                }
            }
            catch
            {
                // Ignore invalid paths
            }
        }

        // Fallback to assuming it's in PATH
        return "wordmd";
    }
}