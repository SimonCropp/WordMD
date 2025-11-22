public class EditorLauncher
{
    WordMDDocument _document;
    EditorInfo _editor;
    MarkdownToWordConverter _converter;
    RiderSettingsGenerator _riderSettings;
    ILogger<EditorLauncher> _logger;
    string? _tempDirectory;
    Process? _editorProcess;
    FileChangeWatcher? _watcher;

    public EditorLauncher(
        WordMDDocument document,
        EditorInfo editor,
        MarkdownToWordConverter converter,
        RiderSettingsGenerator riderSettings,
        ILogger<EditorLauncher> logger)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        _editor = editor ?? throw new ArgumentNullException(nameof(editor));
        _converter = converter ?? throw new ArgumentNullException(nameof(converter));
        _riderSettings = riderSettings ?? throw new ArgumentNullException(nameof(riderSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task LaunchAsync(string docxPath, Cancel cancel = default)
    {
        try
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), "WordMD", Guid.NewGuid().ToString());
            _logger.LogInformation("Using temp directory: {TempDirectory}", _tempDirectory);

            // Extract markdown and images
            _document.ExtractToDirectory(_tempDirectory);

            // Generate Rider settings if using Rider
            if (_editor.Name.Equals("rider", StringComparison.OrdinalIgnoreCase))
            {
                _riderSettings.GenerateSettings(_tempDirectory);
            }

            // Find the markdown file
            var markdownFile = Path.Combine(_tempDirectory, "word.md");
            if (!File.Exists(markdownFile))
            {
                _logger.LogError("No markdown file found. Creating");
                await File.CreateText(markdownFile).DisposeAsync();
            }

            // Start file watcher
            var watcherLogger = _logger; // Could create a specific logger for the watcher
            _watcher = new FileChangeWatcher(
                _tempDirectory,
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
        var executablePath = _editor.GetExecutablePath();
        if (executablePath == null || !File.Exists(executablePath))
        {
            _logger.LogError("Editor executable not found: {EditorName}", _editor.DisplayName);
            return;
        }

        var args = string.Format(_editor.CommandLineArgs, markdownFile);
        _logger.LogInformation("Launching {EditorName}: {Executable} {Args}", _editor.DisplayName, executablePath, args);

        var startInfo = new ProcessStartInfo
        {
            FileName = executablePath,
            Arguments = args,
            UseShellExecute = false,
            CreateNoWindow = false
        };

        _editorProcess = Process.Start(startInfo);
        if (_editorProcess == null)
        {
            _logger.LogError("Failed to start editor process");
            return;
        }

        _logger.LogInformation("Editor process started with PID: {ProcessId}", _editorProcess.Id);

        // Wait for editor to exit
        await _editorProcess.WaitForExitAsync(cancel);
        _logger.LogInformation("Editor process exited");
    }

    private void OnFileChanged(string docxPath, string markdownFile)
    {
        _logger.LogInformation("File change detected, converting markdown to Word");

        try
        {
            // Convert markdown to Word
            _converter.ConvertToWord(markdownFile, docxPath);

            // Re-embed markdown and images
            _document.EmbedFromDirectory(_tempDirectory!);

            _logger.LogInformation("Document updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating document");
        }
    }

    private void Cleanup()
    {
        _logger.LogInformation("Cleaning up temp directory");

        _watcher?.Dispose();
        _editorProcess?.Dispose();

        if (_tempDirectory != null && Directory.Exists(_tempDirectory))
        {
            try
            {
                Directory.Delete(_tempDirectory, true);
                _logger.LogInformation("Temp directory deleted");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete temp directory");
            }
        }
    }
}