namespace WordMD.Core;

public class EditorDetectionService
{
    public List<InstalledEditor> DetectInstalledEditors()
    {
        var installed = new List<InstalledEditor>();

        foreach (var editor in KnownEditors.AllEditors)
        {
            var path = FindEditorPath(editor);
            if (path is not null)
            {
                installed.Add(new InstalledEditor(editor, path));
            }
        }

        return installed;
    }

    private static string? FindEditorPath(EditorDefinition editor)
    {
        foreach (var possiblePath in editor.PossiblePaths)
        {
            var expandedPath = Environment.ExpandEnvironmentVariables(possiblePath);

            // Handle wildcards for versioned installations (like Rider)
            if (expandedPath.Contains('*'))
            {
                var directory = Path.GetDirectoryName(expandedPath);
                var pattern = Path.GetFileName(expandedPath);

                if (directory is not null && Directory.Exists(directory))
                {
                    var matches = Directory.GetFiles(directory, pattern, SearchOption.AllDirectories);
                    if (matches.Length > 0)
                    {
                        return matches.OrderByDescending(f => f).First();
                    }
                }
            }
            else if (File.Exists(expandedPath))
            {
                return expandedPath;
            }
        }

        return null;
    }
}

public record InstalledEditor(EditorDefinition Definition, string ExecutablePath);
