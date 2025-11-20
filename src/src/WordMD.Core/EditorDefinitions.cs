namespace WordMD.Core;

public record EditorDefinition(
    string Name,
    string DisplayName,
    string ExecutableName,
    string[] PossiblePaths,
    string ArgumentsPattern);

public static class KnownEditors
{
    public static readonly EditorDefinition VisualStudioCode = new(
        "vscode",
        "Visual Studio Code",
        "Code.exe",
        [
            @"C:\Program Files\Microsoft VS Code\Code.exe",
            @"C:\Program Files (x86)\Microsoft VS Code\Code.exe",
            @"%LOCALAPPDATA%\Programs\Microsoft VS Code\Code.exe"
        ],
        "\"{0}\"");

    public static readonly EditorDefinition Rider = new(
        "rider",
        "JetBrains Rider",
        "rider64.exe",
        [
            @"C:\Program Files\JetBrains\JetBrains Rider*\bin\rider64.exe"
        ],
        "\"{0}\"");

    public static readonly EditorDefinition Typora = new(
        "typora",
        "Typora",
        "Typora.exe",
        [
            @"C:\Program Files\Typora\Typora.exe",
            @"C:\Program Files (x86)\Typora\Typora.exe"
        ],
        "\"{0}\"");

    public static readonly EditorDefinition MarkdownMonster = new(
        "markdownmonster",
        "Markdown Monster",
        "MarkdownMonster.exe",
        [
            @"C:\Program Files\Markdown Monster\MarkdownMonster.exe"
        ],
        "\"{0}\"");

    public static readonly EditorDefinition Obsidian = new(
        "obsidian",
        "Obsidian",
        "Obsidian.exe",
        [
            @"%LOCALAPPDATA%\Obsidian\Obsidian.exe"
        ],
        "obsidian://open?path={0}");

    public static readonly EditorDefinition Notepad = new(
        "notepad",
        "Notepad",
        "notepad.exe",
        [
            @"C:\Windows\System32\notepad.exe"
        ],
        "\"{0}\"");

    public static IReadOnlyList<EditorDefinition> AllEditors { get; } =
    [
        VisualStudioCode,
        Rider,
        Typora,
        MarkdownMonster,
        Obsidian,
        Notepad
    ];

    public static EditorDefinition? GetByName(string name) =>
        AllEditors.FirstOrDefault(e => 
            e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
}
