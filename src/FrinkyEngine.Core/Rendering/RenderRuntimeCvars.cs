using System.Numerics;

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

    /// <summary>
    /// Optional ambient light override. When non-null, replaces the default 0.15 ambient
    /// (skylights still take priority when present).
    /// </summary>
    public static Vector3? AmbientOverride { get; private set; }

    /// <summary>
    /// Gets the ambient override as "r g b" or "default" if not set.
    /// </summary>
    /// <returns>The current value string.</returns>
    public static string GetAmbientValue()
    {
        if (!AmbientOverride.HasValue)
            return "default (0.15 0.15 0.15)";
        var v = AmbientOverride.Value;
        return $"{v.X:F2} {v.Y:F2} {v.Z:F2}";
    }

    /// <summary>
    /// Attempts to parse and apply the ambient override from "r g b" (0-1 per channel).
    /// Pass "default" to clear the override.
    /// </summary>
    /// <param name="value">User input value.</param>
    /// <returns><c>true</c> if the value was accepted; otherwise <c>false</c>.</returns>
    public static bool TrySetAmbient(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (value.Trim().Equals("default", StringComparison.OrdinalIgnoreCase))
        {
            AmbientOverride = null;
            return true;
        }

        var parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3)
            return false;

        if (!float.TryParse(parts[0], out var r) ||
            !float.TryParse(parts[1], out var g) ||
            !float.TryParse(parts[2], out var b))
            return false;

        if (!float.IsFinite(r) || !float.IsFinite(g) || !float.IsFinite(b))
            return false;

        AmbientOverride = new Vector3(
            Math.Clamp(r, 0f, 1f),
            Math.Clamp(g, 0f, 1f),
            Math.Clamp(b, 0f, 1f));
        return true;
    }

    /// <summary>
    /// Screen percentage (10-200). 100 = native resolution. Values below 100 render at lower
    /// resolution and upscale with nearest-neighbor filtering for a pixelated look.
    /// </summary>
    public static int ScreenPercentage { get; set; } = 100;

    /// <summary>
    /// Gets the screen percentage cvar value as a string.
    /// </summary>
    /// <returns>The current value string.</returns>
    public static string GetScreenPercentageValue() => ScreenPercentage.ToString();

    /// <summary>
    /// Attempts to parse and apply the screen percentage cvar (10-200).
    /// </summary>
    /// <param name="value">User input value.</param>
    /// <returns><c>true</c> if the value was accepted; otherwise <c>false</c>.</returns>
    public static bool TrySetScreenPercentage(string value)
    {
        if (!int.TryParse(value, out var sp) || sp < 10 || sp > 200)
            return false;
        ScreenPercentage = sp;
        return true;
    }

    /// <summary>
    /// Returns scaled dimensions based on the current screen percentage.
    /// </summary>
    /// <param name="displayWidth">The display width in pixels.</param>
    /// <param name="displayHeight">The display height in pixels.</param>
    /// <returns>The scaled width and height.</returns>
    public static (int width, int height) GetScaledDimensions(int displayWidth, int displayHeight)
    {
        if (ScreenPercentage == 100)
            return (displayWidth, displayHeight);
        return (
            Math.Max(1, displayWidth * ScreenPercentage / 100),
            Math.Max(1, displayHeight * ScreenPercentage / 100));
    }

    /// <summary>
    /// Target FPS value (0 = uncapped). Mirrors the value passed to Raylib.SetTargetFPS.
    /// </summary>
    public static int TargetFps { get; set; }

    /// <summary>
    /// Gets the target FPS cvar value as a string.
    /// </summary>
    /// <returns>The current value string.</returns>
    public static string GetTargetFpsValue()
    {
        return TargetFps.ToString();
    }

    /// <summary>
    /// Attempts to parse and apply the target FPS cvar (0-500, 0 = uncapped).
    /// </summary>
    /// <param name="value">User input value.</param>
    /// <returns><c>true</c> if the value was accepted; otherwise <c>false</c>.</returns>
    public static bool TrySetTargetFps(string value)
    {
        if (!int.TryParse(value, out var fps) || fps < 0 || fps > 500)
            return false;

        TargetFps = fps;
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
