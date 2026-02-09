namespace FrinkyEngine.Core.UI.ConsoleSystem;

/// <summary>
/// Describes a console variable (cvar) that can be queried or set from the developer console.
/// </summary>
public sealed class ConsoleCVar
{
    private readonly Func<string> _getter;
    private readonly Func<string, bool> _setter;

    /// <summary>
    /// Creates a new cvar definition.
    /// </summary>
    /// <param name="name">Unique cvar name.</param>
    /// <param name="usage">Usage string shown in help and validation errors.</param>
    /// <param name="description">Human-readable cvar description.</param>
    /// <param name="getter">Callback that returns the current cvar value as text.</param>
    /// <param name="setter">Callback that applies a user-provided value. Returns <c>true</c> on success.</param>
    public ConsoleCVar(
        string name,
        string usage,
        string description,
        Func<string> getter,
        Func<string, bool> setter)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("CVar name cannot be empty.", nameof(name));
        if (name.Any(char.IsWhiteSpace))
            throw new ArgumentException("CVar name cannot contain whitespace.", nameof(name));

        Name = name.Trim();
        Usage = string.IsNullOrWhiteSpace(usage) ? Name : usage.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? "No description." : description.Trim();
        _getter = getter ?? throw new ArgumentNullException(nameof(getter));
        _setter = setter ?? throw new ArgumentNullException(nameof(setter));
    }

    /// <summary>
    /// Unique cvar name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Usage string shown in help and validation errors.
    /// </summary>
    public string Usage { get; }

    /// <summary>
    /// Human-readable cvar description.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the current cvar value as text.
    /// </summary>
    /// <returns>The current value.</returns>
    public string GetValue()
    {
        return _getter();
    }

    /// <summary>
    /// Attempts to set the cvar from user input.
    /// </summary>
    /// <param name="value">Value text from the console input.</param>
    /// <returns><c>true</c> if the value was accepted; otherwise <c>false</c>.</returns>
    public bool TrySetValue(string value)
    {
        return _setter(value);
    }
}
