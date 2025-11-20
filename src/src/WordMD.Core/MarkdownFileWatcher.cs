namespace WordMD.Core;

public class MarkdownFileWatcher : IDisposable
{
    private readonly FileSystemWatcher _watcher;
    private readonly string _directory;
    private readonly Action _onChanged;
    private DateTime _lastChangeTime = DateTime.MinValue;
    private readonly TimeSpan _debounceInterval = TimeSpan.FromMilliseconds(500);

    public MarkdownFileWatcher(string directory, Action onChanged)
    {
        _directory = directory;
        _onChanged = onChanged;

        _watcher = new FileSystemWatcher(directory)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
            EnableRaisingEvents = true,
            IncludeSubdirectories = true
        };

        _watcher.Changed += OnFileSystemChanged;
        _watcher.Created += OnFileSystemChanged;
        _watcher.Deleted += OnFileSystemChanged;
        _watcher.Renamed += OnFileSystemChanged;
    }

    private void OnFileSystemChanged(object sender, FileSystemEventArgs e)
    {
        // Debounce rapid file changes
        var now = DateTime.Now;
        if (now - _lastChangeTime < _debounceInterval)
        {
            return;
        }

        _lastChangeTime = now;

        // Ignore temp files and hidden files
        var fileName = Path.GetFileName(e.FullPath);
        if (fileName.StartsWith('.') || fileName.StartsWith('~') || fileName.EndsWith(".tmp"))
        {
            return;
        }

        _onChanged();
    }

    public void Dispose()
    {
        _watcher.Changed -= OnFileSystemChanged;
        _watcher.Created -= OnFileSystemChanged;
        _watcher.Deleted -= OnFileSystemChanged;
        _watcher.Renamed -= OnFileSystemChanged;
        _watcher.Dispose();
    }
}
