namespace FrinkyEngine.Core.CanvasUI.Authoring;

internal sealed class CanvasHotReloadService
{
    private readonly Dictionary<string, WatchedFile> _watched = new(StringComparer.OrdinalIgnoreCase);
    private readonly TimeSpan _debounce = TimeSpan.FromMilliseconds(180);

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

        var now = DateTime.UtcNow;
        var pendingCallbacks = new List<(string Path, Action<string> Callback)>();
        var keys = _watched.Keys.ToArray();

        foreach (var path in keys)
        {
            var entry = _watched[path];
            var currentWrite = SafeGetLastWriteUtc(path);
            if (currentWrite > entry.LastWriteUtc)
            {
                entry = entry with
                {
                    LastWriteUtc = currentWrite,
                    PendingSinceUtc = now
                };
            }

            if (entry.PendingSinceUtc.HasValue && now - entry.PendingSinceUtc.Value >= _debounce)
            {
                entry = entry with { PendingSinceUtc = null };
                pendingCallbacks.Add((path, entry.Callback));
            }

            _watched[path] = entry;
        }

        foreach (var (path, callback) in pendingCallbacks)
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
