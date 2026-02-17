using System.Numerics;
using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.Rendering;
using Hexa.NET.ImGui;
using Raylib_cs;

namespace FrinkyEngine.Core.UI.Internal;

internal sealed unsafe class ImGuiUiBackend : IUiBackend
{
    private const float DefaultUiFontPixelSize = 16f;
    private const string DefaultUiFontRelativePath = "EngineContent/Fonts/JetBrainsMono-Regular.ttf";

    private ImGuiContextPtr _context;

    private readonly Dictionary<KeyboardKey, ImGuiKey> _raylibKeyMap = new();
    private readonly Dictionary<ImGuiMouseCursor, MouseCursor> _mouseCursorMap = new();
    private readonly Dictionary<int, Texture2D> _managedTextures = new();

    private bool _lastFrameFocused;
    private bool _lastControlPressed;
    private bool _lastShiftPressed;
    private bool _lastAltPressed;
    private bool _lastSuperPressed;

    private float _dt = 1f / 60f;
    private UiFrameDesc _frameDesc = new(1, 1);
    private UiInputCapture _inputCapture;
    private bool _disposed;

    public UiInputCapture InputCapture => _inputCapture;

    public ImGuiUiBackend()
    {
        ImGuiRlRendering.BuildKeyMap(_raylibKeyMap);
        ImGuiRlRendering.BuildCursorMap(_mouseCursorMap);

        var previous = ImGui.GetCurrentContext();
        _context = ImGui.CreateContext();
        ImGui.SetCurrentContext(_context);

        var io = ImGui.GetIO();
        io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors
                         | ImGuiBackendFlags.HasSetMousePos
                         | ImGuiBackendFlags.HasGamepad
                         | ImGuiBackendFlags.RendererHasTextures;
        io.MousePos = new Vector2(0, 0);
        TryConfigureDefaultFont(io);

        ImGui.StyleColorsDark();
        ImGui.SetCurrentContext(previous);
    }

    public void PrepareFrame(float dt, in UiFrameDesc frameDesc)
    {
        _dt = dt > 0f ? dt : 1f / 60f;
        _frameDesc = frameDesc;
    }

    public void RenderFrame(IReadOnlyList<Action<UiContext>> drawCommands, UiContext context)
    {
        if (_disposed)
            return;

        var previous = ImGui.GetCurrentContext();
        ImGui.SetCurrentContext(_context);

        try
        {
            var io = ImGui.GetIO();
            Begin(io);

            for (int i = 0; i < drawCommands.Count; i++)
            {
                try
                {
                    drawCommands[i](context);
                }
                catch (Exception ex)
                {
                    FrinkyLog.Error($"UI draw callback failed: {ex.Message}");
                }
            }

            ImGui.Render();
            _inputCapture = new UiInputCapture(io.WantCaptureMouse, io.WantCaptureKeyboard, io.WantTextInput);
            ImGuiRlRendering.ProcessTextures(_managedTextures);
            ImGuiRlRendering.RenderDrawData(ImGui.GetDrawData());
        }
        finally
        {
            ImGui.SetCurrentContext(previous);
        }
    }

    public void ClearFrame()
    {
        // No-op for now: queue clearing lives in the public UI facade.
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        var previous = ImGui.GetCurrentContext();
        bool restorePrevious = previous.Handle != _context.Handle;
        ImGui.SetCurrentContext(_context);

        foreach (var tex in _managedTextures.Values)
            Raylib.UnloadTexture(tex);
        _managedTextures.Clear();

        ImGui.DestroyContext(_context);
        _context = default;
        _disposed = true;

        if (restorePrevious && previous.Handle != null)
            ImGui.SetCurrentContext(previous);
    }

    private void Begin(ImGuiIOPtr io)
    {
        io.DisplaySize = new Vector2(_frameDesc.ClampedWidth, _frameDesc.ClampedHeight);
        io.DisplayFramebufferScale = new Vector2(1, 1);
        io.DeltaTime = _dt;

        bool focused = _frameDesc.IsFocused;
        if (focused != _lastFrameFocused)
            io.AddFocusEvent(focused);
        _lastFrameFocused = focused;

        if (!_frameDesc.IsHovered)
        {
            io.AddMousePosEvent(-float.MaxValue, -float.MaxValue);
        }
        else if (io.WantSetMousePos && _frameDesc.AllowSetMousePos)
        {
            Raylib.SetMousePosition((int)io.MousePos.X, (int)io.MousePos.Y);
        }
        else if (_frameDesc.UseMousePositionOverride)
        {
            io.AddMousePosEvent(_frameDesc.MousePosition.X, _frameDesc.MousePosition.Y);
        }
        else
        {
            io.AddMousePosEvent(Raylib.GetMouseX(), Raylib.GetMouseY());
        }

        ProcessMouseButton(io, ImGuiMouseButton.Left, MouseButton.Left, _frameDesc.IsHovered);
        ProcessMouseButton(io, ImGuiMouseButton.Right, MouseButton.Right, _frameDesc.IsHovered);
        ProcessMouseButton(io, ImGuiMouseButton.Middle, MouseButton.Middle, _frameDesc.IsHovered);
        ProcessMouseButton(io, (ImGuiMouseButton)3, MouseButton.Side, _frameDesc.IsHovered);
        ProcessMouseButton(io, (ImGuiMouseButton)4, MouseButton.Extra, _frameDesc.IsHovered);

        if (_frameDesc.IsHovered)
        {
            var wheelMove = _frameDesc.UseMouseWheelOverride ? _frameDesc.MouseWheel : Raylib.GetMouseWheelMoveV();
            io.AddMouseWheelEvent(wheelMove.X, wheelMove.Y);
        }

        bool allowKeyboardInput = _frameDesc.AllowKeyboardInput && _frameDesc.IsFocused;

        bool ctrl = allowKeyboardInput &&
                    (Raylib.IsKeyDown(KeyboardKey.LeftControl) || Raylib.IsKeyDown(KeyboardKey.RightControl));
        bool shift = allowKeyboardInput &&
                     (Raylib.IsKeyDown(KeyboardKey.LeftShift) || Raylib.IsKeyDown(KeyboardKey.RightShift));
        bool alt = allowKeyboardInput &&
                   (Raylib.IsKeyDown(KeyboardKey.LeftAlt) || Raylib.IsKeyDown(KeyboardKey.RightAlt));
        bool super = allowKeyboardInput &&
                     (Raylib.IsKeyDown(KeyboardKey.LeftSuper) || Raylib.IsKeyDown(KeyboardKey.RightSuper));

        if (ctrl != _lastControlPressed) io.AddKeyEvent(ImGuiKey.ModCtrl, ctrl);
        if (shift != _lastShiftPressed) io.AddKeyEvent(ImGuiKey.ModShift, shift);
        if (alt != _lastAltPressed) io.AddKeyEvent(ImGuiKey.ModAlt, alt);
        if (super != _lastSuperPressed) io.AddKeyEvent(ImGuiKey.ModSuper, super);

        _lastControlPressed = ctrl;
        _lastShiftPressed = shift;
        _lastAltPressed = alt;
        _lastSuperPressed = super;

        int keyPressed = (int)Raylib.GetKeyPressed();
        while (keyPressed != 0)
        {
            if (allowKeyboardInput && _raylibKeyMap.TryGetValue((KeyboardKey)keyPressed, out var imKey))
                io.AddKeyEvent(imKey, true);

            keyPressed = (int)Raylib.GetKeyPressed();
        }

        if (allowKeyboardInput)
        {
            foreach (var (rlKey, imKey) in _raylibKeyMap)
            {
                if (Raylib.IsKeyReleased(rlKey))
                    io.AddKeyEvent(imKey, false);
            }

            int charPressed = Raylib.GetCharPressed();
            while (charPressed != 0)
            {
                io.AddInputCharacter((uint)charPressed);
                charPressed = Raylib.GetCharPressed();
            }
        }
        else
        {
            // Drain char queue so stale text input is not carried into focused frames.
            while (Raylib.GetCharPressed() != 0)
            {
            }
        }

        if (allowKeyboardInput && io.ConfigFlags.HasFlag(ImGuiConfigFlags.NavEnableGamepad) && Raylib.IsGamepadAvailable(0))
            ImGuiRlRendering.ProcessGamepad(io);

        if (_frameDesc.AllowCursorChanges && !io.ConfigFlags.HasFlag(ImGuiConfigFlags.NoMouseCursorChange))
        {
            var cursor = ImGui.GetMouseCursor();
            if (cursor == ImGuiMouseCursor.None || io.MouseDrawCursor)
            {
                Raylib.HideCursor();
            }
            else
            {
                Raylib.ShowCursor();
                if (_mouseCursorMap.TryGetValue(cursor, out var rlCursor))
                    Raylib.SetMouseCursor(rlCursor);
                else
                    Raylib.SetMouseCursor(MouseCursor.Default);
            }
        }

        ImGui.NewFrame();
    }

    private static void ProcessMouseButton(ImGuiIOPtr io, ImGuiMouseButton imButton, MouseButton rlButton, bool allowPress)
    {
        if (allowPress && Raylib.IsMouseButtonPressed(rlButton))
            io.AddMouseButtonEvent((int)imButton, true);
        if (Raylib.IsMouseButtonReleased(rlButton))
            io.AddMouseButtonEvent((int)imButton, false);
    }

    private static void TryConfigureDefaultFont(ImGuiIOPtr io)
    {
        var fontPath = ResolveDefaultFontPath();
        if (fontPath == null)
        {
            FrinkyLog.Warning($"UI font not found at '{DefaultUiFontRelativePath}'. Falling back to ImGui default font.");
            return;
        }

        try
        {
            var font = io.Fonts.AddFontFromFileTTF(fontPath, DefaultUiFontPixelSize);
            io.FontDefault = font;
            FrinkyLog.Info($"UI default font loaded: {fontPath}");
        }
        catch (Exception ex)
        {
            FrinkyLog.Warning($"Failed to load UI default font '{fontPath}': {ex.Message}. Falling back to ImGui default font.");
        }
    }

    private static string? ResolveDefaultFontPath()
    {
        static string Normalize(string value) => value.Replace('/', Path.DirectorySeparatorChar);

        var candidates = new[]
        {
            Normalize(DefaultUiFontRelativePath),
            Path.Combine(AppContext.BaseDirectory, Normalize(DefaultUiFontRelativePath)),
            Path.Combine(AssetManager.Instance.EngineContentPath, "Fonts", "JetBrainsMono-Regular.ttf")
        };

        foreach (var candidate in candidates)
        {
            try
            {
                var fullPath = Path.GetFullPath(candidate);
                if (File.Exists(fullPath))
                    return fullPath;
            }
            catch
            {
                // Ignore invalid path candidates.
            }
        }

        return null;
    }
}
