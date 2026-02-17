namespace FrinkyEngine.Core.CanvasUI.Authoring;

internal sealed class CanvasHotReloadService
{
    private readonly Dictionary<string, WatchedFile> _watched = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<(string Path, Action<string> Callback)> _pendingCallbacks = new();
    private readonly List<(string Path, WatchedFile Entry)> _updates = new();
    private readonly TimeSpan _debounce = TimeSpan.FromMilliseconds(180);
    private int _frameCounter;

    public bool Enabled { get; set; }

    public void WatchFile(string fullPath, Action<string> onChanged)
    {
        if (string.IsNullOrWhiteSpace(fullPath))
            return;

        fullPath = Path.GetFullPath(fullPath);
        _watched[fullPath] = new WatchedFile(
            LastWriteUtc: SafeGetLastWriteUtc(fullPath),
            PendingSinceUtc: null,
            Callback: onChanged);
    }

    public void Clear()
    {
        _watched.Clear();
    }

    public void Update()
    {
        if (!Enabled || _watched.Count == 0)
            return;

        _frameCounter++;
        bool pollThisFrame = _frameCounter >= 30;
        if (pollThisFrame)
            _frameCounter = 0;

        var now = DateTime.UtcNow;
        _pendingCallbacks.Clear();
        _updates.Clear();

        foreach (var (path, entry) in _watched)
        {
            var updated = entry;

            if (pollThisFrame)
            {
                var currentWrite = SafeGetLastWriteUtc(path);
                if (currentWrite > updated.LastWriteUtc)
                {
                    updated = updated with
                    {
                        LastWriteUtc = currentWrite,
                        PendingSinceUtc = now
                    };
                }
            }

            if (updated.PendingSinceUtc.HasValue && now - updated.PendingSinceUtc.Value >= _debounce)
            {
                updated = updated with { PendingSinceUtc = null };
                _pendingCallbacks.Add((path, updated.Callback));
            }

            if (!updated.Equals(entry))
                _updates.Add((path, updated));
        }

        foreach (var (path, entry) in _updates)
            _watched[path] = entry;

        foreach (var (path, callback) in _pendingCallbacks)
            callback(path);
    }

    private static DateTime SafeGetLastWriteUtc(string fullPath)
    {
        try
        {
            return File.Exists(fullPath)
                ? File.GetLastWriteTimeUtc(fullPath)
                : DateTime.MinValue;
        }
        catch
        {
            return DateTime.MinValue;
        }
    }

    private readonly record struct WatchedFile(
        DateTime LastWriteUtc,
        DateTime? PendingSinceUtc,
        Action<string> Callback);
}
