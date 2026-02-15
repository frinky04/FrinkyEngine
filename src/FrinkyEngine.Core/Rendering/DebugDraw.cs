using System.Numerics;
using FrinkyEngine.Core.UI;

namespace FrinkyEngine.Core.Rendering;

/// <summary>
/// Provides on-screen debug text rendering (similar to Unreal's Print String).
/// Messages are displayed as an overlay list and automatically expire after a duration.
/// </summary>
public static class DebugDraw
{
    /// <summary>
    /// Prints a debug message on screen for the specified duration.
    /// </summary>
    /// <param name="message">The text to display.</param>
    /// <param name="duration">How long the message is visible in seconds. Default is 5 seconds.</param>
    /// <param name="color">RGBA color as a <see cref="Vector4"/> (0-1 per channel). Defaults to green.</param>
    /// <param name="key">
    /// Optional key for replacing messages. If a message with the same key already exists,
    /// it is replaced instead of creating a new entry. Useful for continuously updating values.
    /// </param>
    public static void PrintString(string message, float duration = 5f, Vector4? color = null, string? key = null)
    {
        if (!EngineOverlays.DebugDrawEnabled)
            return;
        EngineOverlays.AddDebugMessage(message, duration, color ?? new Vector4(0.2f, 1f, 0.2f, 1f), key);
    }
}
