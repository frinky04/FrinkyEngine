using System.Numerics;
using System.Runtime.InteropServices;
using Hexa.NET.ImGui;
using Raylib_cs;
using NativeMemory = System.Runtime.InteropServices.NativeMemory;

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

    public static void Setup(bool darkTheme, bool enableDocking)
    {
        BuildKeyMap();
        BuildCursorMap();

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

        if (ctrl != _lastControlPressed) io.AddKeyEvent(ImGuiKey.ModCtrl, ctrl);
        if (shift != _lastShiftPressed) io.AddKeyEvent(ImGuiKey.ModShift, shift);
        if (alt != _lastAltPressed) io.AddKeyEvent(ImGuiKey.ModAlt, alt);
        if (super != _lastSuperPressed) io.AddKeyEvent(ImGuiKey.ModSuper, super);

        _lastControlPressed = ctrl;
        _lastShiftPressed = shift;
        _lastAltPressed = alt;
        _lastSuperPressed = super;

        // Key press events
        int keyPressed = (int)Raylib.GetKeyPressed();
        while (keyPressed != 0)
        {
            if (RaylibKeyMap.TryGetValue((KeyboardKey)keyPressed, out var imKey))
                io.AddKeyEvent(imKey, true);
            keyPressed = (int)Raylib.GetKeyPressed();
        }

        // Key release events
        foreach (var (rlKey, imKey) in RaylibKeyMap)
        {
            if (Raylib.IsKeyReleased(rlKey))
                io.AddKeyEvent(imKey, false);
        }

        // Text input
        int charPressed = Raylib.GetCharPressed();
        while (charPressed != 0)
        {
            io.AddInputCharacter((uint)charPressed);
            charPressed = Raylib.GetCharPressed();
        }

        // Gamepad
        if (io.ConfigFlags.HasFlag(ImGuiConfigFlags.NavEnableGamepad) && Raylib.IsGamepadAvailable(0))
        {
            ProcessGamepad(io);
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
        var drawData = ImGui.GetDrawData();

        ProcessTextures();

        Rlgl.DrawRenderBatchActive();
        Rlgl.DisableBackfaceCulling();

        for (int l = 0; l < drawData.CmdListsCount; l++)
        {
            ImDrawList* commandList = drawData.CmdLists[l];

            for (int cmdIdx = 0; cmdIdx < commandList->CmdBuffer.Size; cmdIdx++)
            {
                var cmd = commandList->CmdBuffer.Data[cmdIdx];

                // Scissor
                EnableScissor(
                    cmd.ClipRect.X - drawData.DisplayPos.X,
                    cmd.ClipRect.Y - drawData.DisplayPos.Y,
                    cmd.ClipRect.Z - (cmd.ClipRect.X - drawData.DisplayPos.X),
                    cmd.ClipRect.W - (cmd.ClipRect.Y - drawData.DisplayPos.Y));

                if (cmd.UserCallback != null)
                {
                    // User callback (cast and invoke)
                    var callback = Marshal.GetDelegateForFunctionPointer<ImDrawCallback>((nint)cmd.UserCallback);
                    callback(commandList, &cmd);
                }
                else
                {
                    var texId = cmd.GetTexID();
                    uint textureId = (uint)texId.Handle;

                    RenderTriangles(
                        cmd.ElemCount,
                        cmd.IdxOffset,
                        commandList->IdxBuffer,
                        commandList->VtxBuffer,
                        textureId);
                }

                // Flush the batch after each draw command so the scissor rect
                // is applied correctly per-command (rlgl batches otherwise render
                // all geometry with only the last scissor rect)
                Rlgl.DrawRenderBatchActive();
            }
        }

        Rlgl.SetTexture(0);
        Rlgl.DisableScissorTest();
        Rlgl.EnableBackfaceCulling();
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ImDrawCallback(ImDrawList* parentList, ImDrawCmd* cmd);

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
        // Y-flip for OpenGL render textures
        var uv0 = new Vector2(0, 0);
        var uv1 = new Vector2(1, -1);

        ImGui.Image(
            new ImTextureRef(null, new ImTextureID((ulong)rt.Texture.Id)),
            new Vector2(rt.Texture.Width, rt.Texture.Height),
            uv0,
            uv1);
    }

    private static void ProcessTextures()
    {
        // Use PlatformIO.Textures (direct vector) instead of DrawData.Textures (pointer-to-vector)
        // Both reference the same texture list, but the PlatformIO access is more reliable in C#
        var platformIO = ImGui.GetPlatformIO();
        var textures = platformIO.Textures;

        for (int i = 0; i < textures.Size; i++)
        {
            var texData = textures.Data[i];
            var status = texData.Status;

            if (status == ImTextureStatus.WantCreate)
            {
                CreateTexture(texData);
            }
            else if (status == ImTextureStatus.WantUpdates)
            {
                UpdateTexture(texData);
            }
            else if (status == ImTextureStatus.WantDestroy)
            {
                DestroyTexture(texData);
            }
        }
    }

    private static void CreateTexture(ImTextureDataPtr texData)
    {
        int width = texData.Width;
        int height = texData.Height;
        int bytesPerPixel = texData.BytesPerPixel;
        byte* pixels = (byte*)texData.GetPixels();

        if (pixels == null || width <= 0 || height <= 0)
            return;

        // Alpha8 textures need conversion to RGBA32 for Raylib
        // (Raylib's Grayscale format maps to luminance, not alpha)
        byte* uploadPixels = pixels;
        bool needsFree = false;

        if (bytesPerPixel == 1)
        {
            int pixelCount = width * height;
            byte* rgba = (byte*)NativeMemory.Alloc((nuint)(pixelCount * 4));
            for (int j = 0; j < pixelCount; j++)
            {
                rgba[j * 4 + 0] = 255; // R
                rgba[j * 4 + 1] = 255; // G
                rgba[j * 4 + 2] = 255; // B
                rgba[j * 4 + 3] = pixels[j]; // A
            }
            uploadPixels = rgba;
            needsFree = true;
        }

        var image = new Image
        {
            Data = uploadPixels,
            Width = width,
            Height = height,
            Mipmaps = 1,
            Format = PixelFormat.UncompressedR8G8B8A8
        };

        var texture = Raylib.LoadTextureFromImage(image);
        Raylib.SetTextureFilter(texture, TextureFilter.Bilinear);

        if (needsFree)
            NativeMemory.Free(uploadPixels);

        _managedTextures[texData.UniqueID] = texture;
        texData.SetTexID(new ImTextureID((ulong)texture.Id));
        texData.SetStatus(ImTextureStatus.Ok);
    }

    private static void UpdateTexture(ImTextureDataPtr texData)
    {
        if (_managedTextures.TryGetValue(texData.UniqueID, out var existing))
        {
            Raylib.UnloadTexture(existing);
        }

        CreateTexture(texData);
    }

    private static void DestroyTexture(ImTextureDataPtr texData)
    {
        if (_managedTextures.TryGetValue(texData.UniqueID, out var texture))
        {
            Raylib.UnloadTexture(texture);
            _managedTextures.Remove(texData.UniqueID);
        }

        texData.SetTexID(default);
        texData.SetStatus(ImTextureStatus.Destroyed);
    }

    private static void RenderTriangles(
        uint count,
        uint indexStart,
        ImVector<ushort> idxBuffer,
        ImVector<ImDrawVert> vtxBuffer,
        uint textureId)
    {
        if (count < 3) return;

        Rlgl.Begin(DrawMode.Triangles);
        Rlgl.SetTexture(textureId);

        for (uint i = 0; i <= count - 3; i += 3)
        {
            if (Rlgl.CheckRenderBatchLimit(3))
            {
                Rlgl.Begin(DrawMode.Triangles);
                Rlgl.SetTexture(textureId);
            }

            for (uint j = 0; j < 3; j++)
            {
                var idx = idxBuffer.Data[indexStart + i + j];
                var vtx = vtxBuffer.Data[idx];

                // Unpack ImU32 color directly (ABGR packed on little-endian)
                byte r = (byte)(vtx.Col);
                byte g = (byte)(vtx.Col >> 8);
                byte b = (byte)(vtx.Col >> 16);
                byte a = (byte)(vtx.Col >> 24);
                Rlgl.Color4ub(r, g, b, a);
                Rlgl.TexCoord2f(vtx.Uv.X, vtx.Uv.Y);
                Rlgl.Vertex2f(vtx.Pos.X, vtx.Pos.Y);
            }
        }

        Rlgl.End();
    }

    private static void EnableScissor(float x, float y, float width, float height)
    {
        var io = ImGui.GetIO();
        Rlgl.EnableScissorTest();

        float displayHeight = io.DisplaySize.Y;
        float scaleX = io.DisplayFramebufferScale.X;
        float scaleY = io.DisplayFramebufferScale.Y;

        Rlgl.Scissor(
            (int)(x * scaleX),
            (int)((displayHeight - (y + height)) * scaleY),
            (int)(width * scaleX),
            (int)(height * scaleY));
    }

    private static void ProcessMouseButton(ImGuiIOPtr io, ImGuiMouseButton imButton, MouseButton rlButton)
    {
        if (Raylib.IsMouseButtonPressed(rlButton))
            io.AddMouseButtonEvent((int)imButton, true);
        else if (Raylib.IsMouseButtonReleased(rlButton))
            io.AddMouseButtonEvent((int)imButton, false);
    }

    private static void ProcessGamepad(ImGuiIOPtr io)
    {
        MapGamepadButton(io, GamepadButton.LeftFaceUp, ImGuiKey.GamepadDpadUp);
        MapGamepadButton(io, GamepadButton.LeftFaceDown, ImGuiKey.GamepadDpadDown);
        MapGamepadButton(io, GamepadButton.LeftFaceLeft, ImGuiKey.GamepadDpadLeft);
        MapGamepadButton(io, GamepadButton.LeftFaceRight, ImGuiKey.GamepadDpadRight);
        MapGamepadButton(io, GamepadButton.RightFaceUp, ImGuiKey.GamepadFaceUp);
        MapGamepadButton(io, GamepadButton.RightFaceDown, ImGuiKey.GamepadFaceDown);
        MapGamepadButton(io, GamepadButton.RightFaceLeft, ImGuiKey.GamepadFaceLeft);
        MapGamepadButton(io, GamepadButton.RightFaceRight, ImGuiKey.GamepadFaceRight);
        MapGamepadButton(io, GamepadButton.LeftTrigger1, ImGuiKey.GamepadL1);
        MapGamepadButton(io, GamepadButton.LeftTrigger2, ImGuiKey.GamepadL2);
        MapGamepadButton(io, GamepadButton.RightTrigger1, ImGuiKey.GamepadR1);
        MapGamepadButton(io, GamepadButton.RightTrigger2, ImGuiKey.GamepadR2);
        MapGamepadButton(io, GamepadButton.MiddleLeft, ImGuiKey.GamepadBack);
        MapGamepadButton(io, GamepadButton.MiddleRight, ImGuiKey.GamepadStart);
        MapGamepadButton(io, GamepadButton.LeftThumb, ImGuiKey.GamepadL3);
        MapGamepadButton(io, GamepadButton.RightThumb, ImGuiKey.GamepadR3);

        const float deadzone = 0.20f;
        MapGamepadAxis(io, GamepadAxis.LeftX, ImGuiKey.GamepadLStickLeft, ImGuiKey.GamepadLStickRight, deadzone);
        MapGamepadAxis(io, GamepadAxis.LeftY, ImGuiKey.GamepadLStickUp, ImGuiKey.GamepadLStickDown, deadzone);
        MapGamepadAxis(io, GamepadAxis.RightX, ImGuiKey.GamepadRStickLeft, ImGuiKey.GamepadRStickRight, deadzone);
        MapGamepadAxis(io, GamepadAxis.RightY, ImGuiKey.GamepadRStickUp, ImGuiKey.GamepadRStickDown, deadzone);
    }

    private static void MapGamepadButton(ImGuiIOPtr io, GamepadButton rlButton, ImGuiKey imKey)
    {
        if (Raylib.IsGamepadButtonPressed(0, rlButton))
            io.AddKeyEvent(imKey, true);
        else if (Raylib.IsGamepadButtonReleased(0, rlButton))
            io.AddKeyEvent(imKey, false);
    }

    private static void MapGamepadAxis(ImGuiIOPtr io, GamepadAxis axis, ImGuiKey negKey, ImGuiKey posKey, float deadzone)
    {
        float value = Raylib.GetGamepadAxisMovement(0, axis);
        io.AddKeyAnalogEvent(negKey, value < -deadzone, value < -deadzone ? -value : 0);
        io.AddKeyAnalogEvent(posKey, value > deadzone, value > deadzone ? value : 0);
    }

    private static void BuildKeyMap()
    {
        RaylibKeyMap.Clear();

        // Letters
        RaylibKeyMap[KeyboardKey.A] = ImGuiKey.A;
        RaylibKeyMap[KeyboardKey.B] = ImGuiKey.B;
        RaylibKeyMap[KeyboardKey.C] = ImGuiKey.C;
        RaylibKeyMap[KeyboardKey.D] = ImGuiKey.D;
        RaylibKeyMap[KeyboardKey.E] = ImGuiKey.E;
        RaylibKeyMap[KeyboardKey.F] = ImGuiKey.F;
        RaylibKeyMap[KeyboardKey.G] = ImGuiKey.G;
        RaylibKeyMap[KeyboardKey.H] = ImGuiKey.H;
        RaylibKeyMap[KeyboardKey.I] = ImGuiKey.I;
        RaylibKeyMap[KeyboardKey.J] = ImGuiKey.J;
        RaylibKeyMap[KeyboardKey.K] = ImGuiKey.K;
        RaylibKeyMap[KeyboardKey.L] = ImGuiKey.L;
        RaylibKeyMap[KeyboardKey.M] = ImGuiKey.M;
        RaylibKeyMap[KeyboardKey.N] = ImGuiKey.N;
        RaylibKeyMap[KeyboardKey.O] = ImGuiKey.O;
        RaylibKeyMap[KeyboardKey.P] = ImGuiKey.P;
        RaylibKeyMap[KeyboardKey.Q] = ImGuiKey.Q;
        RaylibKeyMap[KeyboardKey.R] = ImGuiKey.R;
        RaylibKeyMap[KeyboardKey.S] = ImGuiKey.S;
        RaylibKeyMap[KeyboardKey.T] = ImGuiKey.T;
        RaylibKeyMap[KeyboardKey.U] = ImGuiKey.U;
        RaylibKeyMap[KeyboardKey.V] = ImGuiKey.V;
        RaylibKeyMap[KeyboardKey.W] = ImGuiKey.W;
        RaylibKeyMap[KeyboardKey.X] = ImGuiKey.X;
        RaylibKeyMap[KeyboardKey.Y] = ImGuiKey.Y;
        RaylibKeyMap[KeyboardKey.Z] = ImGuiKey.Z;

        // Numbers
        RaylibKeyMap[KeyboardKey.Zero] = ImGuiKey.Key0;
        RaylibKeyMap[KeyboardKey.One] = ImGuiKey.Key1;
        RaylibKeyMap[KeyboardKey.Two] = ImGuiKey.Key2;
        RaylibKeyMap[KeyboardKey.Three] = ImGuiKey.Key3;
        RaylibKeyMap[KeyboardKey.Four] = ImGuiKey.Key4;
        RaylibKeyMap[KeyboardKey.Five] = ImGuiKey.Key5;
        RaylibKeyMap[KeyboardKey.Six] = ImGuiKey.Key6;
        RaylibKeyMap[KeyboardKey.Seven] = ImGuiKey.Key7;
        RaylibKeyMap[KeyboardKey.Eight] = ImGuiKey.Key8;
        RaylibKeyMap[KeyboardKey.Nine] = ImGuiKey.Key9;

        // Function keys
        RaylibKeyMap[KeyboardKey.F1] = ImGuiKey.F1;
        RaylibKeyMap[KeyboardKey.F2] = ImGuiKey.F2;
        RaylibKeyMap[KeyboardKey.F3] = ImGuiKey.F3;
        RaylibKeyMap[KeyboardKey.F4] = ImGuiKey.F4;
        RaylibKeyMap[KeyboardKey.F5] = ImGuiKey.F5;
        RaylibKeyMap[KeyboardKey.F6] = ImGuiKey.F6;
        RaylibKeyMap[KeyboardKey.F7] = ImGuiKey.F7;
        RaylibKeyMap[KeyboardKey.F8] = ImGuiKey.F8;
        RaylibKeyMap[KeyboardKey.F9] = ImGuiKey.F9;
        RaylibKeyMap[KeyboardKey.F10] = ImGuiKey.F10;
        RaylibKeyMap[KeyboardKey.F11] = ImGuiKey.F11;
        RaylibKeyMap[KeyboardKey.F12] = ImGuiKey.F12;

        // Navigation
        RaylibKeyMap[KeyboardKey.Left] = ImGuiKey.LeftArrow;
        RaylibKeyMap[KeyboardKey.Right] = ImGuiKey.RightArrow;
        RaylibKeyMap[KeyboardKey.Up] = ImGuiKey.UpArrow;
        RaylibKeyMap[KeyboardKey.Down] = ImGuiKey.DownArrow;
        RaylibKeyMap[KeyboardKey.PageUp] = ImGuiKey.PageUp;
        RaylibKeyMap[KeyboardKey.PageDown] = ImGuiKey.PageDown;
        RaylibKeyMap[KeyboardKey.Home] = ImGuiKey.Home;
        RaylibKeyMap[KeyboardKey.End] = ImGuiKey.End;
        RaylibKeyMap[KeyboardKey.Insert] = ImGuiKey.Insert;
        RaylibKeyMap[KeyboardKey.Delete] = ImGuiKey.Delete;
        RaylibKeyMap[KeyboardKey.Backspace] = ImGuiKey.Backspace;
        RaylibKeyMap[KeyboardKey.Enter] = ImGuiKey.Enter;
        RaylibKeyMap[KeyboardKey.Escape] = ImGuiKey.Escape;
        RaylibKeyMap[KeyboardKey.Tab] = ImGuiKey.Tab;
        RaylibKeyMap[KeyboardKey.Space] = ImGuiKey.Space;

        // Modifiers
        RaylibKeyMap[KeyboardKey.LeftShift] = ImGuiKey.LeftShift;
        RaylibKeyMap[KeyboardKey.RightShift] = ImGuiKey.RightShift;
        RaylibKeyMap[KeyboardKey.LeftControl] = ImGuiKey.LeftCtrl;
        RaylibKeyMap[KeyboardKey.RightControl] = ImGuiKey.RightCtrl;
        RaylibKeyMap[KeyboardKey.LeftAlt] = ImGuiKey.LeftAlt;
        RaylibKeyMap[KeyboardKey.RightAlt] = ImGuiKey.RightAlt;
        RaylibKeyMap[KeyboardKey.LeftSuper] = ImGuiKey.LeftSuper;
        RaylibKeyMap[KeyboardKey.RightSuper] = ImGuiKey.RightSuper;

        // Punctuation
        RaylibKeyMap[KeyboardKey.Apostrophe] = ImGuiKey.Apostrophe;
        RaylibKeyMap[KeyboardKey.Comma] = ImGuiKey.Comma;
        RaylibKeyMap[KeyboardKey.Minus] = ImGuiKey.Minus;
        RaylibKeyMap[KeyboardKey.Period] = ImGuiKey.Period;
        RaylibKeyMap[KeyboardKey.Slash] = ImGuiKey.Slash;
        RaylibKeyMap[KeyboardKey.Semicolon] = ImGuiKey.Semicolon;
        RaylibKeyMap[KeyboardKey.Equal] = ImGuiKey.Equal;
        RaylibKeyMap[KeyboardKey.LeftBracket] = ImGuiKey.LeftBracket;
        RaylibKeyMap[KeyboardKey.Backslash] = ImGuiKey.Backslash;
        RaylibKeyMap[KeyboardKey.RightBracket] = ImGuiKey.RightBracket;
        RaylibKeyMap[KeyboardKey.Grave] = ImGuiKey.GraveAccent;

        // Other
        RaylibKeyMap[KeyboardKey.CapsLock] = ImGuiKey.CapsLock;
        RaylibKeyMap[KeyboardKey.ScrollLock] = ImGuiKey.ScrollLock;
        RaylibKeyMap[KeyboardKey.NumLock] = ImGuiKey.NumLock;
        RaylibKeyMap[KeyboardKey.PrintScreen] = ImGuiKey.PrintScreen;
        RaylibKeyMap[KeyboardKey.Pause] = ImGuiKey.Pause;

        // Numpad
        RaylibKeyMap[KeyboardKey.Kp0] = ImGuiKey.Keypad0;
        RaylibKeyMap[KeyboardKey.Kp1] = ImGuiKey.Keypad1;
        RaylibKeyMap[KeyboardKey.Kp2] = ImGuiKey.Keypad2;
        RaylibKeyMap[KeyboardKey.Kp3] = ImGuiKey.Keypad3;
        RaylibKeyMap[KeyboardKey.Kp4] = ImGuiKey.Keypad4;
        RaylibKeyMap[KeyboardKey.Kp5] = ImGuiKey.Keypad5;
        RaylibKeyMap[KeyboardKey.Kp6] = ImGuiKey.Keypad6;
        RaylibKeyMap[KeyboardKey.Kp7] = ImGuiKey.Keypad7;
        RaylibKeyMap[KeyboardKey.Kp8] = ImGuiKey.Keypad8;
        RaylibKeyMap[KeyboardKey.Kp9] = ImGuiKey.Keypad9;
        RaylibKeyMap[KeyboardKey.KpDecimal] = ImGuiKey.KeypadDecimal;
        RaylibKeyMap[KeyboardKey.KpDivide] = ImGuiKey.KeypadDivide;
        RaylibKeyMap[KeyboardKey.KpMultiply] = ImGuiKey.KeypadMultiply;
        RaylibKeyMap[KeyboardKey.KpSubtract] = ImGuiKey.KeypadSubtract;
        RaylibKeyMap[KeyboardKey.KpAdd] = ImGuiKey.KeypadAdd;
        RaylibKeyMap[KeyboardKey.KpEnter] = ImGuiKey.KeypadEnter;
        RaylibKeyMap[KeyboardKey.KpEqual] = ImGuiKey.KeypadEqual;
    }

    private static void BuildCursorMap()
    {
        MouseCursorMap.Clear();
        MouseCursorMap[ImGuiMouseCursor.Arrow] = MouseCursor.Arrow;
        MouseCursorMap[ImGuiMouseCursor.TextInput] = MouseCursor.IBeam;
        MouseCursorMap[ImGuiMouseCursor.ResizeAll] = MouseCursor.ResizeAll;
        MouseCursorMap[ImGuiMouseCursor.ResizeNs] = MouseCursor.ResizeNs;
        MouseCursorMap[ImGuiMouseCursor.ResizeEw] = MouseCursor.ResizeEw;
        MouseCursorMap[ImGuiMouseCursor.ResizeNesw] = MouseCursor.ResizeNesw;
        MouseCursorMap[ImGuiMouseCursor.ResizeNwse] = MouseCursor.ResizeNwse;
        MouseCursorMap[ImGuiMouseCursor.Hand] = MouseCursor.PointingHand;
        MouseCursorMap[ImGuiMouseCursor.NotAllowed] = MouseCursor.NotAllowed;
    }
}
