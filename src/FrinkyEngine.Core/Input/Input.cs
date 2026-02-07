using Raylib_cs;

namespace FrinkyEngine.Core.Input;

/// <summary>
/// Static polling API for keyboard and mouse input. All methods reflect the state for the current frame.
/// </summary>
public static class Input
{
    /// <summary>
    /// Checks whether a keyboard key is currently held down.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns><c>true</c> if the key is held down this frame.</returns>
    public static bool IsKeyDown(KeyboardKey key) => Raylib.IsKeyDown(key);

    /// <summary>
    /// Checks whether a keyboard key was pressed this frame (transition from up to down).
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns><c>true</c> if the key was just pressed.</returns>
    public static bool IsKeyPressed(KeyboardKey key) => Raylib.IsKeyPressed(key);

    /// <summary>
    /// Checks whether a keyboard key was released this frame (transition from down to up).
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns><c>true</c> if the key was just released.</returns>
    public static bool IsKeyReleased(KeyboardKey key) => Raylib.IsKeyReleased(key);

    /// <summary>
    /// Checks whether a keyboard key is currently not pressed.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns><c>true</c> if the key is up this frame.</returns>
    public static bool IsKeyUp(KeyboardKey key) => Raylib.IsKeyUp(key);

    /// <summary>
    /// Checks whether a mouse button is currently held down.
    /// </summary>
    /// <param name="button">The mouse button to check.</param>
    /// <returns><c>true</c> if the button is held down this frame.</returns>
    public static bool IsMouseButtonDown(MouseButton button) => Raylib.IsMouseButtonDown(button);

    /// <summary>
    /// Checks whether a mouse button was pressed this frame.
    /// </summary>
    /// <param name="button">The mouse button to check.</param>
    /// <returns><c>true</c> if the button was just pressed.</returns>
    public static bool IsMouseButtonPressed(MouseButton button) => Raylib.IsMouseButtonPressed(button);

    /// <summary>
    /// Checks whether a mouse button was released this frame.
    /// </summary>
    /// <param name="button">The mouse button to check.</param>
    /// <returns><c>true</c> if the button was just released.</returns>
    public static bool IsMouseButtonReleased(MouseButton button) => Raylib.IsMouseButtonReleased(button);

    /// <summary>
    /// Gets the current mouse cursor position in screen coordinates.
    /// </summary>
    public static System.Numerics.Vector2 MousePosition => Raylib.GetMousePosition();

    /// <summary>
    /// Gets the mouse movement delta since the last frame.
    /// </summary>
    public static System.Numerics.Vector2 MouseDelta => Raylib.GetMouseDelta();

    /// <summary>
    /// Gets the mouse wheel vertical scroll amount for the current frame.
    /// </summary>
    public static float MouseWheelMove => Raylib.GetMouseWheelMove();
}
