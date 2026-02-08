namespace FrinkyEngine.Core.UI;

/// <summary>
/// Disposable helper used for UI begin/end scopes.
/// </summary>
public readonly struct UiScope : IDisposable
{
    private readonly Action? _onDispose;

    internal UiScope(Action? onDispose, bool isVisible)
    {
        _onDispose = onDispose;
        IsVisible = isVisible;
    }

    /// <summary>
    /// Indicates whether the scope's contents should be emitted.
    /// </summary>
    public bool IsVisible { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        _onDispose?.Invoke();
    }
}

