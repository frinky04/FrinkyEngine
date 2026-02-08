using System.Numerics;

namespace FrinkyEngine.Core.UI;

/// <summary>
/// Optional style data that can be applied by widget helpers.
/// </summary>
public readonly struct UiStyle
{
    /// <summary>
    /// Optional text color override.
    /// </summary>
    public Vector4? TextColor { get; init; }

    /// <summary>
    /// Optional text wrap width in pixels; values less than or equal to zero disable wrapping.
    /// </summary>
    public float WrapWidth { get; init; }

    /// <summary>
    /// When <c>true</c>, widget content is drawn in disabled state.
    /// </summary>
    public bool Disabled { get; init; }
}

