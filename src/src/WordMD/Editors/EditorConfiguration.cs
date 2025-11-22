public class EditorConfiguration
{
    const string ConfigFileName = "wordmd-config.json";
    static string configPath;

    static EditorConfiguration()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var wordmdPath = Path.Combine(appDataPath, "WordMD");
        Directory.CreateDirectory(wordmdPath);
        configPath = Path.Combine(wordmdPath, ConfigFileName);
    }

    public static List<string> GetEditorOrder()
    {
        if (!File.Exists(configPath))
        {
            Log.Information("Config file not found, detecting installed editors");
            return DetectInstalledEditors();
        }

        var json = File.ReadAllText(configPath);
        var config = JsonSerializer.Deserialize<ConfigData>(json);
        return config?.EditorOrder ?? DetectInstalledEditors();
    }

    static JsonSerializerOptions jsonSerializerOptions =
        new()
        {
            WriteIndented = true
        };

    public static void SetEditorOrder(List<string> editorOrder)
    {
        Log.Information("Saving editor order: {EditorOrder}", string.Join(", ", editorOrder));

        var config = new ConfigData
        {
            EditorOrder = editorOrder
        };
        var json = JsonSerializer.Serialize(config, jsonSerializerOptions);
        File.WriteAllText(configPath, json);
    }

    public static string? GetDefaultEditor()
    {
        var order = GetEditorOrder();
        return order.FirstOrDefault();
    }

    public static List<string> DetectInstalledEditors()
    {
        var installed = new List<string>();

        foreach (var editor in EditorInfo.AllEditors)
        {
            if (editor.IsInstalled())
            {
                installed.Add(editor.Name);
                Log.Information("Detected installed editor: {EditorName}", editor.DisplayName);
            }
        }

        if (installed.Count == 0)
        {
            Log.Warning("No markdown editors detected");
        }

        return installed;
    }

    public static EditorInfo? GetEditor(string name) =>
        EditorInfo.AllEditors.FirstOrDefault(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    class ConfigData
    {
        public List<string> EditorOrder { get; set; } = [];
    }
}