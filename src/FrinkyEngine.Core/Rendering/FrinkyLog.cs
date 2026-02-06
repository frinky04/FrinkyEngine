namespace FrinkyEngine.Core.Rendering;

public enum LogLevel
{
    Info,
    Warning,
    Error
}

public readonly struct LogEntry
{
    public string Message { get; init; }
    public LogLevel Level { get; init; }
    public DateTime Timestamp { get; init; }
    public string Source { get; init; }
}

public static class FrinkyLog
{
    private static readonly List<LogEntry> _entries = new();
    public static IReadOnlyList<LogEntry> Entries => _entries;

    public static event Action<LogEntry>? OnLog;

    public static void Info(string message) => Log(message, LogLevel.Info);
    public static void Warning(string message) => Log(message, LogLevel.Warning);
    public static void Error(string message) => Log(message, LogLevel.Error);

    public static void Log(string message, LogLevel level) => Log(message, level, "Engine");

    public static void Log(string message, LogLevel level, string source)
    {
        var entry = new LogEntry
        {
            Message = message,
            Level = level,
            Timestamp = DateTime.Now,
            Source = source
        };
        _entries.Add(entry);
        OnLog?.Invoke(entry);
        Console.WriteLine($"[{entry.Timestamp:HH:mm:ss}] [{entry.Level}] [{entry.Source}] {entry.Message}");
    }

    public static void Clear() => _entries.Clear();
}
