using System.Text.Json;

namespace WordMD.Core;

public class ConfigurationService
{
    private static readonly string ConfigDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".wordmd");

    private static readonly string ConfigFilePath = Path.Combine(ConfigDirectory, "config.json");

    public WordMdConfiguration Load()
    {
        if (!File.Exists(ConfigFilePath))
        {
            return new WordMdConfiguration();
        }

        try
        {
            var json = File.ReadAllText(ConfigFilePath);
            return JsonSerializer.Deserialize<WordMdConfiguration>(json) ?? new WordMdConfiguration();
        }
        catch
        {
            return new WordMdConfiguration();
        }
    }

    public void Save(WordMdConfiguration configuration)
    {
        Directory.CreateDirectory(ConfigDirectory);

        var json = JsonSerializer.Serialize(configuration, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(ConfigFilePath, json);
    }

    public void UpdateEditorOrder(List<string> editorOrder)
    {
        var config = Load();
        config.EditorOrder = editorOrder;
        Save(config);
    }
}

public class WordMdConfiguration
{
    public List<string> EditorOrder { get; set; } = [];
    public string? DefaultEditor { get; set; }
}
