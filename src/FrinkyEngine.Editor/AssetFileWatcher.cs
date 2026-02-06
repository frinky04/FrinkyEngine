namespace FrinkyEngine.Editor;

public class AssetFileWatcher : IDisposable
{
    private FileSystemWatcher? _watcher;
    private readonly object _lock = new();
    private bool _changesPending;
    private bool _scriptChangesPending;
    private DateTime _lastEventTime;
    private readonly double _debounceSeconds;
    private readonly HashSet<string> _changedPaths = new(StringComparer.OrdinalIgnoreCase);

    public AssetFileWatcher(double debounceSeconds = 0.5)
    {
        _debounceSeconds = debounceSeconds;
    }

    public void Watch(string directoryPath)
    {
        Stop();

        if (!Directory.Exists(directoryPath))
            return;

        _watcher = new FileSystemWatcher(directoryPath)
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName
                | NotifyFilters.DirectoryName
                | NotifyFilters.LastWrite
                | NotifyFilters.Size
                | NotifyFilters.CreationTime,
            EnableRaisingEvents = true
        };

        _watcher.Created += OnFileEvent;
        _watcher.Deleted += OnFileEvent;
        _watcher.Changed += OnFileEvent;
        _watcher.Renamed += OnRenamedEvent;
        _watcher.Error += OnError;
    }

    public void Stop()
    {
        if (_watcher != null)
        {
            _watcher.Created -= OnFileEvent;
            _watcher.Deleted -= OnFileEvent;
            _watcher.Changed -= OnFileEvent;
            _watcher.Renamed -= OnRenamedEvent;
            _watcher.Error -= OnError;
            _watcher.Dispose();
            _watcher = null;
        }

        lock (_lock)
        {
            _changesPending = false;
            _scriptChangesPending = false;
            _changedPaths.Clear();
        }
    }

    public bool PollChanges(out bool scriptsChanged, out HashSet<string>? changedPaths)
    {
        scriptsChanged = false;
        changedPaths = null;
        lock (_lock)
        {
            if (!_changesPending)
                return false;

            if ((DateTime.UtcNow - _lastEventTime).TotalSeconds < _debounceSeconds)
                return false;

            scriptsChanged = _scriptChangesPending;
            changedPaths = _changedPaths.Count > 0 ? new HashSet<string>(_changedPaths, StringComparer.OrdinalIgnoreCase) : null;
            _changesPending = false;
            _scriptChangesPending = false;
            _changedPaths.Clear();
            return true;
        }
    }

    private void OnFileEvent(object sender, FileSystemEventArgs e)
    {
        RecordChange(e.FullPath);
    }

    private void OnRenamedEvent(object sender, RenamedEventArgs e)
    {
        RecordChange(e.FullPath);
    }

    private void OnError(object sender, ErrorEventArgs e)
    {
        RecordChange();
    }

    private void RecordChange(string? path = null)
    {
        lock (_lock)
        {
            _changesPending = true;
            if (path != null)
            {
                _changedPaths.Add(path);
                if (path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                    _scriptChangesPending = true;
            }
            _lastEventTime = DateTime.UtcNow;
        }
    }

    public void Dispose()
    {
        Stop();
    }
}
