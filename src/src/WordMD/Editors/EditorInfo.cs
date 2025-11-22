namespace WordMD.Editors;

public record EditorInfo(
    string Name,
    string DisplayName,
    string ExecutableName,
    string CommandLineArgs,
    Func<bool> IsInstalled)
{
    public static EditorInfo VSCode { get; } = new(
        "vscode",
        "Visual Studio Code",
        "code",
        "\"{0}\"",
        () => FindInPath("code") != null || FindInPath("code.cmd") != null);

    public static EditorInfo Rider { get; } = new(
        "rider",
        "JetBrains Rider",
        "rider64.exe",
        "\"{0}\"",
        () => FindInPath("rider64.exe") != null || Directory.Exists(@"C:\Program Files\JetBrains\JetBrains Rider"));

    public static EditorInfo Notepad { get; } = new(
        "notepad",
        "Notepad++",
        "notepad++.exe",
        "\"{0}\"",
        () => FindInPath("notepad++.exe") != null || File.Exists(@"C:\Program Files\Notepad++\notepad++.exe"));

    public static EditorInfo Typora { get; } = new(
        "typora",
        "Typora",
        "typora.exe",
        "\"{0}\"",
        () => FindInPath("typora.exe") != null || File.Exists(@"C:\Program Files\Typora\Typora.exe"));

    public static EditorInfo MarkdownMonster { get; } = new(
        "markdownmonster",
        "Markdown Monster",
        "MarkdownMonster.exe",
        "\"{0}\"",
        () => FindInPath("MarkdownMonster.exe") != null || File.Exists(@"C:\Program Files\Markdown Monster\MarkdownMonster.exe"));

    public static EditorInfo Obsidian { get; } = new(
        "obsidian",
        "Obsidian",
        "obsidian.exe",
        "\"{0}\"",
        () => FindInPath("obsidian.exe") != null);

    public static EditorInfo[] AllEditors { get; } =
    [
        VSCode,
        Rider,
        Notepad,
        Typora,
        MarkdownMonster,
        Obsidian
    ];

    private static string? FindInPath(string fileName)
    {
        var paths = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ?? [];

        foreach (var path in paths)
        {
            try
            {
                var fullPath = Path.Combine(path, fileName);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }
            catch
            {
                // Ignore invalid paths
            }
        }

        return null;
    }

    public string? GetExecutablePath()
    {
        // Try to find in PATH first
        var pathResult = FindInPath(ExecutableName);
        if (pathResult != null)
        {
            return pathResult;
        }

        // Try common installation locations
        return Name switch
        {
            "rider" => FindRiderPath(),
            "notepad" => @"C:\Program Files\Notepad++\notepad++.exe",
            "typora" => @"C:\Program Files\Typora\Typora.exe",
            "markdownmonster" => @"C:\Program Files\Markdown Monster\MarkdownMonster.exe",
            _ => null
        };
    }

    private static string? FindRiderPath()
    {
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var riderDir = Path.Combine(programFiles, "JetBrains");

        if (!Directory.Exists(riderDir))
        {
            return null;
        }

        var versions = Directory.GetDirectories(riderDir, "JetBrains Rider*");
        if (versions.Length == 0)
        {
            return null;
        }

        var latestVersion = versions.OrderByDescending(v => v).First();
        var exePath = Path.Combine(latestVersion, "bin", "rider64.exe");

        return File.Exists(exePath) ? exePath : null;
    }
}