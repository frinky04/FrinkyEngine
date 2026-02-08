using System.Numerics;
using System.Runtime.InteropServices;
using FrinkyEngine.Core.Rendering;
using Hexa.NET.ImGui;
using Raylib_cs;
using NativeMemory = System.Runtime.InteropServices.NativeMemory;

namespace FrinkyEngine.Core.UI.Internal;

internal sealed unsafe class ImGuiUiBackend : IUiBackend
{
    private const float DefaultUiFontPixelSize = 16f;
    private const string DefaultUiFontRelativePath = "EditorAssets/Fonts/JetBrains_Mono/static/JetBrainsMono-Regular.ttf";

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
        BuildKeyMap();
        BuildCursorMap();

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
            ProcessTextures();
            RenderDrawData(ImGui.GetDrawData());
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
            ProcessGamepad(io);

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

    private void RenderDrawData(ImDrawDataPtr drawData)
    {
        Rlgl.DrawRenderBatchActive();
        Rlgl.DisableBackfaceCulling();

        for (int l = 0; l < drawData.CmdListsCount; l++)
        {
            ImDrawList* commandList = drawData.CmdLists[l];

            for (int cmdIdx = 0; cmdIdx < commandList->CmdBuffer.Size; cmdIdx++)
            {
                var cmd = commandList->CmdBuffer.Data[cmdIdx];
                EnableScissor(
                    cmd.ClipRect.X - drawData.DisplayPos.X,
                    cmd.ClipRect.Y - drawData.DisplayPos.Y,
                    cmd.ClipRect.Z - (cmd.ClipRect.X - drawData.DisplayPos.X),
                    cmd.ClipRect.W - (cmd.ClipRect.Y - drawData.DisplayPos.Y));

                if (cmd.UserCallback != null)
                {
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

                // Flush each draw command so scissor changes apply correctly.
                Rlgl.DrawRenderBatchActive();
            }
        }

        Rlgl.SetTexture(0);
        Rlgl.DisableScissorTest();
        Rlgl.EnableBackfaceCulling();
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ImDrawCallback(ImDrawList* parentList, ImDrawCmd* cmd);

    private void ProcessTextures()
    {
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

    private void CreateTexture(ImTextureDataPtr texData)
    {
        int width = texData.Width;
        int height = texData.Height;
        int bytesPerPixel = texData.BytesPerPixel;
        byte* pixels = (byte*)texData.GetPixels();

        if (pixels == null || width <= 0 || height <= 0)
            return;

        byte* uploadPixels = pixels;
        bool needsFree = false;

        // ImGui may provide monochrome alpha-mask textures either as Alpha8
        // or as RGBA32 with UseColors=false. Convert both forms to white RGB
        // + mask alpha to avoid dark fringes on glyph edges.
        bool useAlphaMask = bytesPerPixel == 1
                            || texData.Format == ImTextureFormat.Alpha8
                            || !texData.UseColors;
        if (useAlphaMask)
        {
            int pixelCount = width * height;
            byte* rgba = (byte*)NativeMemory.Alloc((nuint)(pixelCount * 4));
            int maskChannel = DetermineMaskChannel(pixels, pixelCount, bytesPerPixel);
            for (int j = 0; j < pixelCount; j++)
            {
                byte alpha;
                if (bytesPerPixel <= 1)
                {
                    alpha = pixels[j];
                }
                else
                {
                    int baseIndex = j * bytesPerPixel;
                    alpha = pixels[baseIndex + maskChannel];
                }

                rgba[j * 4 + 0] = 255;
                rgba[j * 4 + 1] = 255;
                rgba[j * 4 + 2] = 255;
                rgba[j * 4 + 3] = alpha;
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

    private static int DetermineMaskChannel(byte* pixels, int pixelCount, int bytesPerPixel)
    {
        if (bytesPerPixel <= 1)
            return 0;

        int channelCount = Math.Min(bytesPerPixel, 4);
        int sampleCount = Math.Min(pixelCount, 4096);
        int stride = Math.Max(1, pixelCount / sampleCount);

        int bestChannel = Math.Min(channelCount - 1, 3);
        float bestScore = float.NegativeInfinity;

        for (int ch = 0; ch < channelCount; ch++)
        {
            byte min = 255;
            byte max = 0;

            for (int i = 0; i < sampleCount; i++)
            {
                int px = i * stride;
                int idx = px * bytesPerPixel + ch;
                byte v = pixels[idx];
                if (v < min) min = v;
                if (v > max) max = v;
            }

            if (min == max)
                continue;

            float score = max - min;
            if (ch == 3)
                score += 24f;
            if (min == 0 && max == 255)
                score += 8f;

            if (score > bestScore)
            {
                bestScore = score;
                bestChannel = ch;
            }
        }

        return bestChannel;
    }

    private void UpdateTexture(ImTextureDataPtr texData)
    {
        if (_managedTextures.TryGetValue(texData.UniqueID, out var existing))
            Raylib.UnloadTexture(existing);

        CreateTexture(texData);
    }

    private void DestroyTexture(ImTextureDataPtr texData)
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
        if (count < 3)
            return;

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

    private static void ProcessMouseButton(ImGuiIOPtr io, ImGuiMouseButton imButton, MouseButton rlButton, bool allowPress)
    {
        if (allowPress && Raylib.IsMouseButtonPressed(rlButton))
            io.AddMouseButtonEvent((int)imButton, true);
        if (Raylib.IsMouseButtonReleased(rlButton))
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
        io.AddKeyAnalogEvent(negKey, value < -deadzone, value < -deadzone ? -value : 0f);
        io.AddKeyAnalogEvent(posKey, value > deadzone, value > deadzone ? value : 0f);
    }

    private void BuildKeyMap()
    {
        _raylibKeyMap.Clear();

        _raylibKeyMap[KeyboardKey.A] = ImGuiKey.A;
        _raylibKeyMap[KeyboardKey.B] = ImGuiKey.B;
        _raylibKeyMap[KeyboardKey.C] = ImGuiKey.C;
        _raylibKeyMap[KeyboardKey.D] = ImGuiKey.D;
        _raylibKeyMap[KeyboardKey.E] = ImGuiKey.E;
        _raylibKeyMap[KeyboardKey.F] = ImGuiKey.F;
        _raylibKeyMap[KeyboardKey.G] = ImGuiKey.G;
        _raylibKeyMap[KeyboardKey.H] = ImGuiKey.H;
        _raylibKeyMap[KeyboardKey.I] = ImGuiKey.I;
        _raylibKeyMap[KeyboardKey.J] = ImGuiKey.J;
        _raylibKeyMap[KeyboardKey.K] = ImGuiKey.K;
        _raylibKeyMap[KeyboardKey.L] = ImGuiKey.L;
        _raylibKeyMap[KeyboardKey.M] = ImGuiKey.M;
        _raylibKeyMap[KeyboardKey.N] = ImGuiKey.N;
        _raylibKeyMap[KeyboardKey.O] = ImGuiKey.O;
        _raylibKeyMap[KeyboardKey.P] = ImGuiKey.P;
        _raylibKeyMap[KeyboardKey.Q] = ImGuiKey.Q;
        _raylibKeyMap[KeyboardKey.R] = ImGuiKey.R;
        _raylibKeyMap[KeyboardKey.S] = ImGuiKey.S;
        _raylibKeyMap[KeyboardKey.T] = ImGuiKey.T;
        _raylibKeyMap[KeyboardKey.U] = ImGuiKey.U;
        _raylibKeyMap[KeyboardKey.V] = ImGuiKey.V;
        _raylibKeyMap[KeyboardKey.W] = ImGuiKey.W;
        _raylibKeyMap[KeyboardKey.X] = ImGuiKey.X;
        _raylibKeyMap[KeyboardKey.Y] = ImGuiKey.Y;
        _raylibKeyMap[KeyboardKey.Z] = ImGuiKey.Z;

        _raylibKeyMap[KeyboardKey.Zero] = ImGuiKey.Key0;
        _raylibKeyMap[KeyboardKey.One] = ImGuiKey.Key1;
        _raylibKeyMap[KeyboardKey.Two] = ImGuiKey.Key2;
        _raylibKeyMap[KeyboardKey.Three] = ImGuiKey.Key3;
        _raylibKeyMap[KeyboardKey.Four] = ImGuiKey.Key4;
        _raylibKeyMap[KeyboardKey.Five] = ImGuiKey.Key5;
        _raylibKeyMap[KeyboardKey.Six] = ImGuiKey.Key6;
        _raylibKeyMap[KeyboardKey.Seven] = ImGuiKey.Key7;
        _raylibKeyMap[KeyboardKey.Eight] = ImGuiKey.Key8;
        _raylibKeyMap[KeyboardKey.Nine] = ImGuiKey.Key9;

        _raylibKeyMap[KeyboardKey.F1] = ImGuiKey.F1;
        _raylibKeyMap[KeyboardKey.F2] = ImGuiKey.F2;
        _raylibKeyMap[KeyboardKey.F3] = ImGuiKey.F3;
        _raylibKeyMap[KeyboardKey.F4] = ImGuiKey.F4;
        _raylibKeyMap[KeyboardKey.F5] = ImGuiKey.F5;
        _raylibKeyMap[KeyboardKey.F6] = ImGuiKey.F6;
        _raylibKeyMap[KeyboardKey.F7] = ImGuiKey.F7;
        _raylibKeyMap[KeyboardKey.F8] = ImGuiKey.F8;
        _raylibKeyMap[KeyboardKey.F9] = ImGuiKey.F9;
        _raylibKeyMap[KeyboardKey.F10] = ImGuiKey.F10;
        _raylibKeyMap[KeyboardKey.F11] = ImGuiKey.F11;
        _raylibKeyMap[KeyboardKey.F12] = ImGuiKey.F12;

        _raylibKeyMap[KeyboardKey.Left] = ImGuiKey.LeftArrow;
        _raylibKeyMap[KeyboardKey.Right] = ImGuiKey.RightArrow;
        _raylibKeyMap[KeyboardKey.Up] = ImGuiKey.UpArrow;
        _raylibKeyMap[KeyboardKey.Down] = ImGuiKey.DownArrow;
        _raylibKeyMap[KeyboardKey.PageUp] = ImGuiKey.PageUp;
        _raylibKeyMap[KeyboardKey.PageDown] = ImGuiKey.PageDown;
        _raylibKeyMap[KeyboardKey.Home] = ImGuiKey.Home;
        _raylibKeyMap[KeyboardKey.End] = ImGuiKey.End;
        _raylibKeyMap[KeyboardKey.Insert] = ImGuiKey.Insert;
        _raylibKeyMap[KeyboardKey.Delete] = ImGuiKey.Delete;
        _raylibKeyMap[KeyboardKey.Backspace] = ImGuiKey.Backspace;
        _raylibKeyMap[KeyboardKey.Enter] = ImGuiKey.Enter;
        _raylibKeyMap[KeyboardKey.Escape] = ImGuiKey.Escape;
        _raylibKeyMap[KeyboardKey.Tab] = ImGuiKey.Tab;
        _raylibKeyMap[KeyboardKey.Space] = ImGuiKey.Space;

        _raylibKeyMap[KeyboardKey.LeftShift] = ImGuiKey.LeftShift;
        _raylibKeyMap[KeyboardKey.RightShift] = ImGuiKey.RightShift;
        _raylibKeyMap[KeyboardKey.LeftControl] = ImGuiKey.LeftCtrl;
        _raylibKeyMap[KeyboardKey.RightControl] = ImGuiKey.RightCtrl;
        _raylibKeyMap[KeyboardKey.LeftAlt] = ImGuiKey.LeftAlt;
        _raylibKeyMap[KeyboardKey.RightAlt] = ImGuiKey.RightAlt;
        _raylibKeyMap[KeyboardKey.LeftSuper] = ImGuiKey.LeftSuper;
        _raylibKeyMap[KeyboardKey.RightSuper] = ImGuiKey.RightSuper;

        _raylibKeyMap[KeyboardKey.Apostrophe] = ImGuiKey.Apostrophe;
        _raylibKeyMap[KeyboardKey.Comma] = ImGuiKey.Comma;
        _raylibKeyMap[KeyboardKey.Minus] = ImGuiKey.Minus;
        _raylibKeyMap[KeyboardKey.Period] = ImGuiKey.Period;
        _raylibKeyMap[KeyboardKey.Slash] = ImGuiKey.Slash;
        _raylibKeyMap[KeyboardKey.Semicolon] = ImGuiKey.Semicolon;
        _raylibKeyMap[KeyboardKey.Equal] = ImGuiKey.Equal;
        _raylibKeyMap[KeyboardKey.LeftBracket] = ImGuiKey.LeftBracket;
        _raylibKeyMap[KeyboardKey.Backslash] = ImGuiKey.Backslash;
        _raylibKeyMap[KeyboardKey.RightBracket] = ImGuiKey.RightBracket;
        _raylibKeyMap[KeyboardKey.Grave] = ImGuiKey.GraveAccent;

        _raylibKeyMap[KeyboardKey.CapsLock] = ImGuiKey.CapsLock;
        _raylibKeyMap[KeyboardKey.ScrollLock] = ImGuiKey.ScrollLock;
        _raylibKeyMap[KeyboardKey.NumLock] = ImGuiKey.NumLock;
        _raylibKeyMap[KeyboardKey.PrintScreen] = ImGuiKey.PrintScreen;
        _raylibKeyMap[KeyboardKey.Pause] = ImGuiKey.Pause;

        _raylibKeyMap[KeyboardKey.Kp0] = ImGuiKey.Keypad0;
        _raylibKeyMap[KeyboardKey.Kp1] = ImGuiKey.Keypad1;
        _raylibKeyMap[KeyboardKey.Kp2] = ImGuiKey.Keypad2;
        _raylibKeyMap[KeyboardKey.Kp3] = ImGuiKey.Keypad3;
        _raylibKeyMap[KeyboardKey.Kp4] = ImGuiKey.Keypad4;
        _raylibKeyMap[KeyboardKey.Kp5] = ImGuiKey.Keypad5;
        _raylibKeyMap[KeyboardKey.Kp6] = ImGuiKey.Keypad6;
        _raylibKeyMap[KeyboardKey.Kp7] = ImGuiKey.Keypad7;
        _raylibKeyMap[KeyboardKey.Kp8] = ImGuiKey.Keypad8;
        _raylibKeyMap[KeyboardKey.Kp9] = ImGuiKey.Keypad9;
        _raylibKeyMap[KeyboardKey.KpDecimal] = ImGuiKey.KeypadDecimal;
        _raylibKeyMap[KeyboardKey.KpDivide] = ImGuiKey.KeypadDivide;
        _raylibKeyMap[KeyboardKey.KpMultiply] = ImGuiKey.KeypadMultiply;
        _raylibKeyMap[KeyboardKey.KpSubtract] = ImGuiKey.KeypadSubtract;
        _raylibKeyMap[KeyboardKey.KpAdd] = ImGuiKey.KeypadAdd;
        _raylibKeyMap[KeyboardKey.KpEnter] = ImGuiKey.KeypadEnter;
        _raylibKeyMap[KeyboardKey.KpEqual] = ImGuiKey.KeypadEqual;
    }

    private void BuildCursorMap()
    {
        _mouseCursorMap.Clear();
        _mouseCursorMap[ImGuiMouseCursor.Arrow] = MouseCursor.Arrow;
        _mouseCursorMap[ImGuiMouseCursor.TextInput] = MouseCursor.IBeam;
        _mouseCursorMap[ImGuiMouseCursor.ResizeAll] = MouseCursor.ResizeAll;
        _mouseCursorMap[ImGuiMouseCursor.ResizeNs] = MouseCursor.ResizeNs;
        _mouseCursorMap[ImGuiMouseCursor.ResizeEw] = MouseCursor.ResizeEw;
        _mouseCursorMap[ImGuiMouseCursor.ResizeNesw] = MouseCursor.ResizeNesw;
        _mouseCursorMap[ImGuiMouseCursor.ResizeNwse] = MouseCursor.ResizeNwse;
        _mouseCursorMap[ImGuiMouseCursor.Hand] = MouseCursor.PointingHand;
        _mouseCursorMap[ImGuiMouseCursor.NotAllowed] = MouseCursor.NotAllowed;
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
            Path.Combine(AppContext.BaseDirectory, Normalize(DefaultUiFontRelativePath))
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
