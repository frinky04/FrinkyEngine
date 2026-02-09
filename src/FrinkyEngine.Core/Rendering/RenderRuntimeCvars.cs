namespace FrinkyEngine.Core.Rendering;

/// <summary>
/// Runtime rendering cvars controlled by the developer console.
/// </summary>
public static class RenderRuntimeCvars
{
    /// <summary>
    /// Global post-processing toggle used by standalone runtime and editor Play/Simulate rendering.
    /// </summary>
    public static bool PostProcessingEnabled { get; private set; } = true;

    /// <summary>
    /// Gets the post-processing cvar value as "1" (enabled) or "0" (disabled).
    /// </summary>
    /// <returns>The current value string.</returns>
    public static string GetPostProcessingValue()
    {
        return PostProcessingEnabled ? "1" : "0";
    }

    /// <summary>
    /// Attempts to parse and apply the post-processing cvar from "1" or "0".
    /// </summary>
    /// <param name="value">User input value.</param>
    /// <returns><c>true</c> if the value was accepted; otherwise <c>false</c>.</returns>
    public static bool TrySetPostProcessing(string value)
    {
        if (!TryParseBool01(value, out var enabled))
            return false;

        PostProcessingEnabled = enabled;
        return true;
    }

    private static bool TryParseBool01(string value, out bool enabled)
    {
        if (value == null)
        {
            enabled = false;
            return false;
        }

        switch (value.Trim())
        {
            case "1":
                enabled = true;
                return true;
            case "0":
                enabled = false;
                return true;
            default:
                enabled = false;
                return false;
        }
    }
}
