using System.Numerics;

namespace FrinkyEngine.Core.UI;

/// <summary>
/// Describes the target viewport and input behavior for a UI frame.
/// </summary>
/// <param name="Width">Viewport width in pixels.</param>
/// <param name="Height">Viewport height in pixels.</param>
/// <param name="IsFocused">When <c>true</c>, keyboard focus is considered active.</param>
/// <param name="IsHovered">When <c>true</c>, mouse input is considered active for the UI viewport.</param>
/// <param name="UseMousePositionOverride">When <c>true</c>, <see cref="MousePosition"/> is used instead of screen mouse coordinates.</param>
/// <param name="MousePosition">Mouse position in viewport-local coordinates when override is enabled.</param>
/// <param name="UseMouseWheelOverride">When <c>true</c>, <see cref="MouseWheel"/> is used instead of runtime mouse wheel values.</param>
/// <param name="MouseWheel">Mouse wheel delta to use when override is enabled.</param>
/// <param name="AllowCursorChanges">When <c>true</c>, UI may change the OS cursor shape/visibility.</param>
/// <param name="AllowSetMousePos">When <c>true</c>, UI may reposition the OS cursor if requested.</param>
/// <param name="AllowKeyboardInput">When <c>true</c>, keyboard and text events are forwarded to UI.</param>
public readonly record struct UiFrameDesc(
    int Width,
    int Height,
    bool IsFocused = true,
    bool IsHovered = true,
    bool UseMousePositionOverride = false,
    Vector2 MousePosition = default,
    bool UseMouseWheelOverride = false,
    Vector2 MouseWheel = default,
    bool AllowCursorChanges = true,
    bool AllowSetMousePos = true,
    bool AllowKeyboardInput = true)
{
    /// <summary>
    /// Gets the width clamped to a minimum of 1.
    /// </summary>
    public int ClampedWidth => Math.Max(1, Width);

    /// <summary>
    /// Gets the height clamped to a minimum of 1.
    /// </summary>
    public int ClampedHeight => Math.Max(1, Height);
}

