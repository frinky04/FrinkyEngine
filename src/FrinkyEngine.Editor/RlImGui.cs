using System.Numerics;
using FrinkyEngine.Core.Rendering;
using FrinkyEngine.Core.UI;
using FrinkyEngine.Core.UI.Internal;
using Hexa.NET.ImGui;
using Raylib_cs;

namespace FrinkyEngine.Editor;

/// <summary>
/// Custom Raylib-ImGui integration layer using Hexa.NET.ImGui.
/// Ported from rlImGui-cs to work with Hexa.NET.ImGui types.
/// </summary>
public static unsafe class RlImGui
{
    private static ImGuiContextPtr _context;

    private static readonly Dictionary<KeyboardKey, ImGuiKey> RaylibKeyMap = new();
    private static readonly Dictionary<ImGuiMouseCursor, MouseCursor> MouseCursorMap = new();

    private static readonly Dictionary<int, Texture2D> _managedTextures = new();

    private static bool _lastFrameFocused;
    private static bool _lastControlPressed;
    private static bool _lastShiftPressed;
    private static bool _lastAltPressed;
    private static bool _lastSuperPressed;
    private static bool _deferringKeyboardToGameUi;

    public static void Setup(bool darkTheme, bool enableDocking)
    {
        ImGuiRlRendering.BuildKeyMap(RaylibKeyMap);
        ImGuiRlRendering.BuildCursorMap(MouseCursorMap);

        _context = ImGui.CreateContext();
        ImGui.SetCurrentContext(_context);

        var io = ImGui.GetIO();
        io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors
                         | ImGuiBackendFlags.HasSetMousePos
                         | ImGuiBackendFlags.HasGamepad
                         | ImGuiBackendFlags.RendererHasTextures;

        io.MousePos = new Vector2(0, 0);

        if (enableDocking)
            io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;

        if (darkTheme)
            ImGui.StyleColorsDark();
        else
            ImGui.StyleColorsLight();

        ReloadFonts();
    }

    public static void Begin(float dt)
    {
        ImGui.SetCurrentContext(_context);

        var io = ImGui.GetIO();

        // Display size
        io.DisplaySize = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
        io.DisplayFramebufferScale = new Vector2(1, 1);

        // Delta time
        io.DeltaTime = dt > 0 ? dt : 1.0f / 60.0f;

        // Focus tracking
        bool focused = Raylib.IsWindowFocused();
        if (focused != _lastFrameFocused)
            io.AddFocusEvent(focused);
        _lastFrameFocused = focused;

        // Mouse position
        if (io.WantSetMousePos)
        {
            Raylib.SetMousePosition((int)io.MousePos.X, (int)io.MousePos.Y);
        }
        else
        {
            io.AddMousePosEvent(Raylib.GetMouseX(), Raylib.GetMouseY());
        }

        // Mouse buttons
        ProcessMouseButton(io, ImGuiMouseButton.Left, MouseButton.Left);
        ProcessMouseButton(io, ImGuiMouseButton.Right, MouseButton.Right);
        ProcessMouseButton(io, ImGuiMouseButton.Middle, MouseButton.Middle);
        ProcessMouseButton(io, (ImGuiMouseButton)3, MouseButton.Side);
        ProcessMouseButton(io, (ImGuiMouseButton)4, MouseButton.Extra);

        // Mouse wheel
        var wheelMove = Raylib.GetMouseWheelMoveV();
        io.AddMouseWheelEvent(wheelMove.X, wheelMove.Y);

        // Modifier keys (track transitions)
        bool ctrl = Raylib.IsKeyDown(KeyboardKey.LeftControl) || Raylib.IsKeyDown(KeyboardKey.RightControl);
        bool shift = Raylib.IsKeyDown(KeyboardKey.LeftShift) || Raylib.IsKeyDown(KeyboardKey.RightShift);
        bool alt = Raylib.IsKeyDown(KeyboardKey.LeftAlt) || Raylib.IsKeyDown(KeyboardKey.RightAlt);
        bool super = Raylib.IsKeyDown(KeyboardKey.LeftSuper) || Raylib.IsKeyDown(KeyboardKey.RightSuper);
        bool deferKeyboardToGameUi = EngineOverlays.IsConsoleVisible;

        if (deferKeyboardToGameUi != _deferringKeyboardToGameUi)
        {
            _deferringKeyboardToGameUi = deferKeyboardToGameUi;
            FrinkyLog.Info(
                deferKeyboardToGameUi
                    ? "RlImGui: deferring key/char queue to in-game UI console."
                    : "RlImGui: resumed key/char queue consumption.");
        }

        if (ctrl != _lastControlPressed) io.AddKeyEvent(ImGuiKey.ModCtrl, ctrl);
        if (shift != _lastShiftPressed) io.AddKeyEvent(ImGuiKey.ModShift, shift);
        if (alt != _lastAltPressed) io.AddKeyEvent(ImGuiKey.ModAlt, alt);
        if (super != _lastSuperPressed) io.AddKeyEvent(ImGuiKey.ModSuper, super);

        _lastControlPressed = ctrl;
        _lastShiftPressed = shift;
        _lastAltPressed = alt;
        _lastSuperPressed = super;

        // Key press events
        if (!deferKeyboardToGameUi)
        {
            int keyPressed = (int)Raylib.GetKeyPressed();
            while (keyPressed != 0)
            {
                if (RaylibKeyMap.TryGetValue((KeyboardKey)keyPressed, out var imKey))
                    io.AddKeyEvent(imKey, true);
                keyPressed = (int)Raylib.GetKeyPressed();
            }
        }

        // Key release events
        foreach (var (rlKey, imKey) in RaylibKeyMap)
        {
            if (Raylib.IsKeyReleased(rlKey))
                io.AddKeyEvent(imKey, false);
        }

        // Text input
        if (!deferKeyboardToGameUi)
        {
            int charPressed = Raylib.GetCharPressed();
            while (charPressed != 0)
            {
                io.AddInputCharacter((uint)charPressed);
                charPressed = Raylib.GetCharPressed();
            }
        }

        // Gamepad
        if (io.ConfigFlags.HasFlag(ImGuiConfigFlags.NavEnableGamepad) && Raylib.IsGamepadAvailable(0))
        {
            ImGuiRlRendering.ProcessGamepad(io);
        }

        // Mouse cursor
        if (!io.ConfigFlags.HasFlag(ImGuiConfigFlags.NoMouseCursorChange))
        {
            var cursor = ImGui.GetMouseCursor();
            if (cursor == ImGuiMouseCursor.None || io.MouseDrawCursor)
            {
                Raylib.HideCursor();
            }
            else
            {
                Raylib.ShowCursor();
                if (MouseCursorMap.TryGetValue(cursor, out var rlCursor))
                    Raylib.SetMouseCursor(rlCursor);
                else
                    Raylib.SetMouseCursor(MouseCursor.Default);
            }
        }

        ImGui.NewFrame();
    }

    public static void End()
    {
        ImGui.Render();

        ImGuiRlRendering.ProcessTextures(_managedTextures);
        ImGuiRlRendering.RenderDrawData(ImGui.GetDrawData());
    }

    public static void Shutdown()
    {
        foreach (var tex in _managedTextures.Values)
            Raylib.UnloadTexture(tex);
        _managedTextures.Clear();

        ImGui.DestroyContext(_context);
    }

    public static void ReloadFonts()
    {
        ImGui.SetCurrentContext(_context);
        var io = ImGui.GetIO();

        // Font atlas build happens automatically in Hexa.NET.ImGui
    }

    public static void ImageRenderTexture(RenderTexture2D rt)
    {
        // Y-flip for OpenGL render textures using normalized UV range.
        var uv0 = new Vector2(0, 1);
        var uv1 = new Vector2(1, 0);

        ImGui.Image(
            new ImTextureRef(null, new ImTextureID((ulong)rt.Texture.Id)),
            new Vector2(rt.Texture.Width, rt.Texture.Height),
            uv0,
            uv1);
    }

    private static void ProcessMouseButton(ImGuiIOPtr io, ImGuiMouseButton imButton, MouseButton rlButton)
    {
        if (Raylib.IsMouseButtonPressed(rlButton))
            io.AddMouseButtonEvent((int)imButton, true);
        else if (Raylib.IsMouseButtonReleased(rlButton))
            io.AddMouseButtonEvent((int)imButton, false);
    }
}
