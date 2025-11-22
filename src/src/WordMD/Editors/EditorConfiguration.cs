using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace WordMD.Editors;

public class EditorConfiguration
{
    private const string ConfigFileName = "wordmd-config.json";
    private readonly string _configPath;
    private readonly ILogger<EditorConfiguration> _logger;

    public EditorConfiguration(ILogger<EditorConfiguration> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var wordmdPath = Path.Combine(appDataPath, "WordMD");
        Directory.CreateDirectory(wordmdPath);
        _configPath = Path.Combine(wordmdPath, ConfigFileName);
    }

    public List<string> GetEditorOrder()
    {
        if (!File.Exists(_configPath))
        {
            _logger.LogInformation("Config file not found, detecting installed editors");
            return DetectInstalledEditors();
        }

        try
        {
            var json = File.ReadAllText(_configPath);
            var config = JsonSerializer.Deserialize<ConfigData>(json);
            return config?.EditorOrder ?? DetectInstalledEditors();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading config file");
            return DetectInstalledEditors();
        }
    }

    public void SetEditorOrder(List<string> editorOrder)
    {
        _logger.LogInformation("Saving editor order: {EditorOrder}", string.Join(", ", editorOrder));
        
        var config = new ConfigData { EditorOrder = editorOrder };
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_configPath, json);
    }

    public string GetDefaultEditor()
    {
        var order = GetEditorOrder();
        return order.FirstOrDefault() ?? EditorInfo.VSCode.Name;
    }

    public List<string> DetectInstalledEditors()
    {
        var installed = new List<string>();
        
        foreach (var editor in EditorInfo.AllEditors)
        {
            if (editor.IsInstalled())
            {
                installed.Add(editor.Name);
                _logger.LogInformation("Detected installed editor: {EditorName}", editor.DisplayName);
            }
        }
        
        if (installed.Count == 0)
        {
            _logger.LogWarning("No markdown editors detected, using VSCode as default");
            installed.Add(EditorInfo.VSCode.Name);
        }
        
        return installed;
    }

    public EditorInfo? GetEditor(string name)
    {
        return EditorInfo.AllEditors.FirstOrDefault(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    private class ConfigData
    {
        public List<string> EditorOrder { get; set; } = [];
    }
}
