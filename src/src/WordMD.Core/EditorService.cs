using System.Diagnostics;

namespace WordMD.Core;

public class EditorService
{
    private readonly DocxMarkdownService _docxService;
    private readonly MarkdownToWordService _markdownToWordService;
    private readonly RiderSettingsGenerator _riderSettingsGenerator;

    public EditorService(
        DocxMarkdownService docxService,
        MarkdownToWordService markdownToWordService,
        RiderSettingsGenerator riderSettingsGenerator)
    {
        _docxService = docxService;
        _markdownToWordService = markdownToWordService;
        _riderSettingsGenerator = riderSettingsGenerator;
    }

    public async Task EditDocumentAsync(
        string docxPath,
        InstalledEditor editor,
        CancellationToken cancellationToken = default)
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), $"wordmd_{Guid.NewGuid()}");

        try
        {
            // Extract markdown and images to temp directory
            _docxService.ExtractToDirectory(docxPath, tempDirectory);

            // Generate Rider settings if using Rider
            if (editor.Definition.Name.Equals("rider", StringComparison.OrdinalIgnoreCase))
            {
                _riderSettingsGenerator.GenerateDotSettings(tempDirectory);
            }

            // Launch editor
            var markdownPath = Path.Combine(tempDirectory, "document.md");
            var editorProcess = LaunchEditor(editor, markdownPath);

            if (editorProcess is null)
            {
                throw new InvalidOperationException($"Failed to launch editor: {editor.Definition.DisplayName}");
            }

            // Watch for file changes
            using var watcher = new MarkdownFileWatcher(tempDirectory, () =>
            {
                SaveChangesToDocx(docxPath, tempDirectory);
            });

            // Wait for editor to close
            await editorProcess.WaitForExitAsync(cancellationToken);

            // Final save
            SaveChangesToDocx(docxPath, tempDirectory);
        }
        finally
        {
            // Cleanup temp directory
            if (Directory.Exists(tempDirectory))
            {
                try
                {
                    Directory.Delete(tempDirectory, true);
                }
                catch
                {
                    // Best effort cleanup
                }
            }
        }
    }

    private void SaveChangesToDocx(string docxPath, string tempDirectory)
    {
        // Read updated markdown and images from temp directory
        var (markdown, images) = _docxService.ReadFromDirectory(tempDirectory);

        // Convert markdown to Word
        _markdownToWordService.EmbedMarkdown(docxPath, markdown);

        // Re-embed markdown and images
        _docxService.EmbedMarkdown(docxPath, markdown, images);
    }

    private Process? LaunchEditor(InstalledEditor editor, string filePath)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = editor.ExecutablePath,
            UseShellExecute = true
        };

        // Format arguments based on editor's pattern
        var arguments = string.Format(editor.Definition.ArgumentsPattern, filePath);
        
        // Handle special case for Obsidian URI scheme
        if (editor.Definition.Name.Equals("obsidian", StringComparison.OrdinalIgnoreCase))
        {
            startInfo.FileName = arguments;
        }
        else
        {
            startInfo.Arguments = arguments;
        }

        try
        {
            return Process.Start(startInfo);
        }
        catch
        {
            return null;
        }
    }
}
