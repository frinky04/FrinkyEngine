using System.Numerics;

namespace FrinkyEngine.Core.Rendering;

/// <summary>
/// Provides on-screen debug text rendering (similar to Unreal's Print String).
/// Messages are displayed as an overlay list and automatically expire after a duration.
/// <para>
/// In the editor, messages render in the viewport overlay. In the runtime build,
/// all methods are no-ops unless a backend is registered.
/// </para>
/// </summary>
public static class DebugDraw
{
    /// <summary>
    /// Backend interface that renders debug screen messages. Implemented by the editor.
    /// </summary>
    public interface IDebugDrawBackend
    {
        /// <summary>
        /// Displays a debug message on screen.
        /// </summary>
        void PrintString(string message, float duration, Vector4 color, string? key);

        /// <summary>
        /// Removes all currently displayed debug messages.
        /// </summary>
        void Clear();
    }

    private static IDebugDrawBackend? _backend;

    /// <summary>
    /// Registers the debug draw backend. Called by the editor during initialization.
    /// </summary>
    /// <param name="backend">The backend implementation, or <c>null</c> to unregister.</param>
    public static void SetBackend(IDebugDrawBackend? backend) => _backend = backend;

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
        _backend?.PrintString(message, duration, color ?? new Vector4(0.2f, 1f, 0.2f, 1f), key);
    }

    /// <summary>
    /// Removes all currently displayed debug messages.
    /// </summary>
    public static void Clear()
    {
        _backend?.Clear();
    }
}
