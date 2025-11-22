public class FileChangeWatcher : IDisposable
{
    readonly string _directory;
    readonly FileSystemWatcher _watcher;
    readonly Action _onChangeCallback;
    DateTime _lastChangeTime = DateTime.MinValue;
    readonly TimeSpan _debounceInterval = TimeSpan.FromMilliseconds(500);

    public FileChangeWatcher(string directory, Action onChangeCallback)
    {
        _directory = directory ?? throw new ArgumentNullException(nameof(directory));
        _onChangeCallback = onChangeCallback ?? throw new ArgumentNullException(nameof(onChangeCallback));

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

        Log.Information("Started watching directory: {Directory}", _directory);
    }

    void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        // Debounce rapid file changes
        var now = DateTime.Now;
        if (now - _lastChangeTime < _debounceInterval)
        {
            return;
        }

        _lastChangeTime = now;
        Log.Information("File {ChangeType}: {FileName}", e.ChangeType, e.Name);

        _onChangeCallback();
    }

    void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        Log.Information("File renamed from {OldName} to {NewName}", e.OldName, e.Name);

        _onChangeCallback();
    }

    public void Dispose()
    {
        _watcher.EnableRaisingEvents = false;
        _watcher.Changed -= OnFileChanged;
        _watcher.Created -= OnFileChanged;
        _watcher.Deleted -= OnFileChanged;
        _watcher.Renamed -= OnFileRenamed;
        _watcher.Dispose();

        Log.Information("Stopped watching directory: {Directory}", _directory);
    }
}
