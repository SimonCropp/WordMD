public class EditorLauncher
{
    WordMDDocument document;
    EditorInfo editor;
    MarkdownToWordConverter converter;
    string? tempDirectory;
    Process? editorProcess;
    FileChangeWatcher? watcher;

    public EditorLauncher(
        WordMDDocument document,
        EditorInfo editor,
        MarkdownToWordConverter converter)
    {
        this.document = document ?? throw new ArgumentNullException(nameof(document));
        this.editor = editor ?? throw new ArgumentNullException(nameof(editor));
        this.converter = converter ?? throw new ArgumentNullException(nameof(converter));
    }

    public async Task LaunchAsync(string docxPath, Cancel cancel = default)
    {
        try
        {
            tempDirectory = Path.Combine(Path.GetTempPath(), "WordMD", Guid.NewGuid().ToString());
            Log.Information("Using temp directory: {TempDirectory}", tempDirectory);

            // Extract markdown and images
            document.ExtractToDirectory(tempDirectory);

            // Generate Rider settings if using Rider
            if (editor.Name.Equals("rider", StringComparison.OrdinalIgnoreCase))
            {
                RiderSettingsGenerator.GenerateSettings(tempDirectory);
            }

            // Find the markdown file
            var markdownFile = Path.Combine(tempDirectory, "word.md");
            if (!File.Exists(markdownFile))
            {
                Log.Error("No markdown file found. Creating");
                await File.CreateText(markdownFile).DisposeAsync();
            }

            watcher = new FileChangeWatcher(
                tempDirectory,
                () => OnFileChanged(docxPath, markdownFile));

            // Launch editor
            await LaunchEditorAsync(markdownFile, cancel);
        }
        finally
        {
            Cleanup();
        }
    }

    async Task LaunchEditorAsync(string markdownFile, Cancel cancel)
    {
        var executablePath = editor.GetExecutablePath();
        if (executablePath == null || !File.Exists(executablePath))
        {
            Log.Error("Editor executable not found: {EditorName}", editor.DisplayName);
            return;
        }

        var args = string.Format(editor.CommandLineArgs, markdownFile);
        Log.Information("Launching {EditorName}: {Executable} {Args}", editor.DisplayName, executablePath, args);

        var startInfo = new ProcessStartInfo
        {
            FileName = executablePath,
            Arguments = args,
            UseShellExecute = false,
            CreateNoWindow = false
        };

        editorProcess = Process.Start(startInfo);
        if (editorProcess == null)
        {
            Log.Error("Failed to start editor process");
            return;
        }

        Log.Information("Editor process started with PID: {ProcessId}", editorProcess.Id);

        // Wait for editor to exit
        await editorProcess.WaitForExitAsync(cancel);
        Log.Information("Editor process exited");
    }

    void OnFileChanged(string docxPath, string markdownFile)
    {
        Log.Information("File change detected, converting markdown to Word");

        // Convert markdown to Word
        converter.ConvertToWord(markdownFile, docxPath);

        // Re-embed markdown and images
        document.EmbedFromDirectory(tempDirectory!);

        Log.Information("Document updated successfully");
    }

    void Cleanup()
    {
        Log.Information("Cleaning up temp directory");

        watcher?.Dispose();
        editorProcess?.Dispose();

        if (tempDirectory != null && Directory.Exists(tempDirectory))
        {
            Directory.Delete(tempDirectory, true);
            Log.Information("Temp directory deleted");
        }
    }
}