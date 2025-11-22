using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace WordMD.Editors;

public class RegistryManager
{
    private const string ContextMenuPath = @"SOFTWARE\Classes\.docx\shell";
    private readonly ILogger<RegistryManager> _logger;

    public RegistryManager(ILogger<RegistryManager> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void RegisterContextMenu(List<string> editorOrder)
    {
        _logger.LogInformation("Registering context menu for WordMD");
        
        try
        {
            // Remove existing entries first
            RemoveContextMenu();
            
            // Get the path to wordmd.exe
            var wordmdPath = GetWordMDPath();
            
            // Register "WordMD Edit" (default editor)
            RegisterDefaultEdit(wordmdPath);
            
            // Register "WordMD" with submenu
            RegisterSubmenu(wordmdPath, editorOrder);
            
            _logger.LogInformation("Context menu registered successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register context menu");
            throw;
        }
    }

    public void RemoveContextMenu()
    {
        _logger.LogInformation("Removing WordMD context menu entries");
        
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(ContextMenuPath, true);
            if (key == null)
            {
                return;
            }
            
            // Remove WordMD Edit
            key.DeleteSubKeyTree("WordMD Edit", false);
            
            // Remove WordMD submenu
            key.DeleteSubKeyTree("WordMD", false);
            
            _logger.LogInformation("Context menu entries removed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing context menu entries");
        }
    }

    private void RegisterDefaultEdit(string wordmdPath)
    {
        using var key = Registry.CurrentUser.CreateSubKey($@"{ContextMenuPath}\WordMD Edit");
        key.SetValue("", "Edit with WordMD");
        key.SetValue("Icon", wordmdPath);
        
        using var commandKey = key.CreateSubKey("command");
        commandKey.SetValue("", $"\"{wordmdPath}\" \"%1\"");
    }

    private void RegisterSubmenu(string wordmdPath, List<string> editorOrder)
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

    private static string GetWordMDPath()
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
