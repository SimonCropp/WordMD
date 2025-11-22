public class FileChangeWatcher : IDisposable
{
    private readonly string _directory;
    private readonly ILogger<EditorLauncher> _logger;
    private readonly FileSystemWatcher _watcher;
    private readonly Action _onChangeCallback;
    private DateTime _lastChangeTime = DateTime.MinValue;
    private readonly TimeSpan _debounceInterval = TimeSpan.FromMilliseconds(500);

    public FileChangeWatcher(string directory, Action onChangeCallback, ILogger<EditorLauncher> logger)
    {
        _directory = directory ?? throw new ArgumentNullException(nameof(directory));
        _onChangeCallback = onChangeCallback ?? throw new ArgumentNullException(nameof(onChangeCallback));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _watcher = new FileSystemWatcher(_directory)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
            EnableRaisingEvents = true,
            IncludeSubdirectories = false
        };

        _watcher.Changed += OnFileChanged;
        _watcher.Created += OnFileChanged;
        _watcher.Deleted += OnFileChanged;
        _watcher.Renamed += OnFileRenamed;

        _logger.LogInformation("Started watching directory: {Directory}", _directory);
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        // Debounce rapid file changes
        var now = DateTime.Now;
        if (now - _lastChangeTime < _debounceInterval)
        {
            return;
        }

        _lastChangeTime = now;
        _logger.LogInformation("File {ChangeType}: {FileName}", e.ChangeType, e.Name);

        try
        {
            _onChangeCallback();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in change callback");
        }
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        _logger.LogInformation("File renamed from {OldName} to {NewName}", e.OldName, e.Name);

        try
        {
            _onChangeCallback();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in change callback");
        }
    }

    public void Dispose()
    {
        _watcher.EnableRaisingEvents = false;
        _watcher.Changed -= OnFileChanged;
        _watcher.Created -= OnFileChanged;
        _watcher.Deleted -= OnFileChanged;
        _watcher.Renamed -= OnFileRenamed;
        _watcher.Dispose();

        _logger.LogInformation("Stopped watching directory: {Directory}", _directory);
    }
}
