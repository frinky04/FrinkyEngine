using System.Numerics;

namespace FrinkyEngine.Core.UI;

/// <summary>
/// Configuration options for <see cref="UiContext.Panel(string,UiPanelOptions)"/>.
/// </summary>
public readonly struct UiPanelOptions
{
    /// <summary>
    /// Optional panel position in pixels.
    /// </summary>
    public Vector2? Position { get; init; }

    /// <summary>
    /// Optional panel size in pixels.
    /// </summary>
    public Vector2? Size { get; init; }

    /// <summary>
    /// Whether to show a title bar. Defaults to <c>false</c>.
    /// </summary>
    public bool HasTitleBar { get; init; }

    /// <summary>
    /// Whether the panel can be moved. Defaults to <c>false</c>.
    /// </summary>
    public bool Movable { get; init; }

    /// <summary>
    /// Whether the panel can be resized. Defaults to <c>false</c>.
    /// </summary>
    public bool Resizable { get; init; }

    /// <summary>
    /// When <c>true</c>, panel background is not rendered.
    /// </summary>
    public bool NoBackground { get; init; }

    /// <summary>
    /// Whether panel should auto-resize to fit contents. Defaults to <c>false</c>.
    /// </summary>
    public bool AutoResize { get; init; }

    /// <summary>
    /// Whether to show a scrollbar when content overflows. Defaults to <c>false</c>.
    /// </summary>
    public bool Scrollbar { get; init; }
}
