namespace FrinkyEngine.Core.UI.ConsoleSystem;

/// <summary>
/// Result of executing a developer-console command or cvar.
/// </summary>
public readonly struct ConsoleExecutionResult
{
    /// <summary>
    /// Creates a new execution result.
    /// </summary>
    /// <param name="success">Whether execution succeeded.</param>
    /// <param name="lines">Output lines to print in console history.</param>
    public ConsoleExecutionResult(bool success, IReadOnlyList<string>? lines = null)
    {
        Success = success;
        Lines = lines ?? Array.Empty<string>();
    }

    /// <summary>
    /// Whether execution succeeded.
    /// </summary>
    public bool Success { get; }

    /// <summary>
    /// Output lines to print in console history.
    /// </summary>
    public IReadOnlyList<string> Lines { get; }
}
