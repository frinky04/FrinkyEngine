namespace FrinkyEngine.Core.UI.ConsoleSystem;

/// <summary>
/// Distinguishes between command and cvar entries in the console registry.
/// </summary>
public enum ConsoleEntryKind : byte
{
    /// <summary>
    /// A callable console command.
    /// </summary>
    Command = 0,

    /// <summary>
    /// A query/set console variable.
    /// </summary>
    CVar = 1
}

/// <summary>
/// Immutable descriptor of one registered console command or cvar entry.
/// </summary>
public readonly struct ConsoleEntryDescriptor
{
    /// <summary>
    /// Creates a new console entry descriptor.
    /// </summary>
    /// <param name="kind">Entry kind.</param>
    /// <param name="name">Primary entry name.</param>
    /// <param name="usage">Usage/help text.</param>
    /// <param name="description">Human-readable description.</param>
    public ConsoleEntryDescriptor(ConsoleEntryKind kind, string name, string usage, string description)
    {
        Kind = kind;
        Name = name ?? string.Empty;
        Usage = usage ?? string.Empty;
        Description = description ?? string.Empty;
    }

    /// <summary>
    /// Entry kind.
    /// </summary>
    public ConsoleEntryKind Kind { get; }

    /// <summary>
    /// Primary entry name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Usage/help text.
    /// </summary>
    public string Usage { get; }

    /// <summary>
    /// Human-readable description.
    /// </summary>
    public string Description { get; }
}
