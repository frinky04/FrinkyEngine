namespace FrinkyEngine.Core.UI;

/// <summary>
/// Indicates whether UI wants to capture player input for the current frame.
/// </summary>
/// <param name="WantsMouse">When <c>true</c>, UI is consuming mouse input.</param>
/// <param name="WantsKeyboard">When <c>true</c>, UI is consuming keyboard input.</param>
/// <param name="WantsTextInput">When <c>true</c>, UI is actively receiving text input.</param>
public readonly record struct UiInputCapture(bool WantsMouse, bool WantsKeyboard, bool WantsTextInput)
{
    /// <summary>
    /// Gets whether any capture flag is active.
    /// </summary>
    public bool Any => WantsMouse || WantsKeyboard || WantsTextInput;

    /// <summary>
    /// Gets a value where no input is captured.
    /// </summary>
    public static UiInputCapture None => default;
}

