public class EditorLauncher
{
    WordMDDocument document;
    EditorInfo editor;
    MarkdownToWordConverter converter;
    RiderSettingsGenerator riderSettings;
    ILogger<EditorLauncher> logger;
    string? tempDirectory;
    Process? editorProcess;
    FileChangeWatcher? watcher;

    public EditorLauncher(
        WordMDDocument document,
        EditorInfo editor,
        MarkdownToWordConverter converter,
        RiderSettingsGenerator riderSettings,
        ILogger<EditorLauncher> logger)
    {
        this.document = document ?? throw new ArgumentNullException(nameof(document));
        this.editor = editor ?? throw new ArgumentNullException(nameof(editor));
        this.converter = converter ?? throw new ArgumentNullException(nameof(converter));
        this.riderSettings = riderSettings ?? throw new ArgumentNullException(nameof(riderSettings));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task LaunchAsync(string docxPath, Cancel cancel = default)
    {
        try
        {
            tempDirectory = Path.Combine(Path.GetTempPath(), "WordMD", Guid.NewGuid().ToString());
            logger.LogInformation("Using temp directory: {TempDirectory}", tempDirectory);

            // Extract markdown and images
            document.ExtractToDirectory(tempDirectory);

            // Generate Rider settings if using Rider
            if (editor.Name.Equals("rider", StringComparison.OrdinalIgnoreCase))
            {
                riderSettings.GenerateSettings(tempDirectory);
            }

            // Find the markdown file
            var markdownFile = Path.Combine(tempDirectory, "word.md");
            if (!File.Exists(markdownFile))
            {
                logger.LogError("No markdown file found. Creating");
                await File.CreateText(markdownFile).DisposeAsync();
            }

            // Start file watcher
            var watcherLogger = logger; // Could create a specific logger for the watcher
            watcher = new FileChangeWatcher(
                tempDirectory,
                () => OnFileChanged(docxPath, markdownFile),
                watcherLogger);

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
            logger.LogError("Editor executable not found: {EditorName}", editor.DisplayName);
            return;
        }

        var args = string.Format(editor.CommandLineArgs, markdownFile);
        logger.LogInformation("Launching {EditorName}: {Executable} {Args}", editor.DisplayName, executablePath, args);

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
            logger.LogError("Failed to start editor process");
            return;
        }

        logger.LogInformation("Editor process started with PID: {ProcessId}", editorProcess.Id);

        // Wait for editor to exit
        await editorProcess.WaitForExitAsync(cancel);
        logger.LogInformation("Editor process exited");
    }

    private void OnFileChanged(string docxPath, string markdownFile)
    {
        logger.LogInformation("File change detected, converting markdown to Word");

        try
        {
            // Convert markdown to Word
            converter.ConvertToWord(markdownFile, docxPath);

            // Re-embed markdown and images
            document.EmbedFromDirectory(tempDirectory!);

            logger.LogInformation("Document updated successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating document");
        }
    }

    private void Cleanup()
    {
        logger.LogInformation("Cleaning up temp directory");

        watcher?.Dispose();
        editorProcess?.Dispose();

        if (tempDirectory != null && Directory.Exists(tempDirectory))
        {
            try
            {
                Directory.Delete(tempDirectory, true);
                logger.LogInformation("Temp directory deleted");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to delete temp directory");
            }
        }
    }
}