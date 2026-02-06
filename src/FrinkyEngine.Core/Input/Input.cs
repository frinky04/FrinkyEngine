using Raylib_cs;

namespace FrinkyEngine.Core.Input;

public static class Input
{
    public static bool IsKeyDown(KeyboardKey key) => Raylib.IsKeyDown(key);
    public static bool IsKeyPressed(KeyboardKey key) => Raylib.IsKeyPressed(key);
    public static bool IsKeyReleased(KeyboardKey key) => Raylib.IsKeyReleased(key);
    public static bool IsKeyUp(KeyboardKey key) => Raylib.IsKeyUp(key);

    public static bool IsMouseButtonDown(MouseButton button) => Raylib.IsMouseButtonDown(button);
    public static bool IsMouseButtonPressed(MouseButton button) => Raylib.IsMouseButtonPressed(button);
    public static bool IsMouseButtonReleased(MouseButton button) => Raylib.IsMouseButtonReleased(button);

    public static System.Numerics.Vector2 MousePosition => Raylib.GetMousePosition();
    public static System.Numerics.Vector2 MouseDelta => Raylib.GetMouseDelta();
    public static float MouseWheelMove => Raylib.GetMouseWheelMove();
}
