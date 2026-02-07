namespace FrinkyEngine.Core.Rendering;

/// <summary>
/// Severity level for log messages.
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// Informational message.
    /// </summary>
    Info,

    /// <summary>
    /// Non-critical issue that may need attention.
    /// </summary>
    Warning,

    /// <summary>
    /// A failure or critical problem.
    /// </summary>
    Error
}

/// <summary>
/// An immutable log message with metadata.
/// </summary>
public readonly struct LogEntry
{
    /// <summary>
    /// The log message text.
    /// </summary>
    public string Message { get; init; }

    /// <summary>
    /// Severity level of this entry.
    /// </summary>
    public LogLevel Level { get; init; }

    /// <summary>
    /// When this entry was created.
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// The subsystem that produced this entry (e.g. "Engine", "Raylib").
    /// </summary>
    public string Source { get; init; }
}

/// <summary>
/// Central logging API for the engine. Messages are stored in memory and broadcast via <see cref="OnLog"/>.
/// </summary>
public static class FrinkyLog
{
    private static readonly List<LogEntry> _entries = new();

    /// <summary>
    /// All log entries recorded so far.
    /// </summary>
    public static IReadOnlyList<LogEntry> Entries => _entries;

    /// <summary>
    /// Raised whenever a new log entry is recorded.
    /// </summary>
    public static event Action<LogEntry>? OnLog;

    /// <summary>
    /// Logs an informational message.
    /// </summary>
    /// <param name="message">The message text.</param>
    public static void Info(string message) => Log(message, LogLevel.Info);

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">The message text.</param>
    public static void Warning(string message) => Log(message, LogLevel.Warning);

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="message">The message text.</param>
    public static void Error(string message) => Log(message, LogLevel.Error);

    /// <summary>
    /// Logs a message with the specified level and "Engine" as the source.
    /// </summary>
    /// <param name="message">The message text.</param>
    /// <param name="level">Severity level.</param>
    public static void Log(string message, LogLevel level) => Log(message, level, "Engine");

    /// <summary>
    /// Logs a message with the specified level and source.
    /// </summary>
    /// <param name="message">The message text.</param>
    /// <param name="level">Severity level.</param>
    /// <param name="source">The subsystem producing the message.</param>
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

    /// <summary>
    /// Removes all stored log entries.
    /// </summary>
    public static void Clear() => _entries.Clear();
}
