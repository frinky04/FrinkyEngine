using System.Numerics;
using System.Runtime.InteropServices;
using Hexa.NET.ImGui;
using Raylib_cs;
using NativeMemory = System.Runtime.InteropServices.NativeMemory;

namespace FrinkyEngine.Core.UI.Internal;

/// <summary>
/// Shared low-level ImGui rendering helpers for Raylib backends.
/// Used by both the runtime <see cref="ImGuiUiBackend"/> and the editor RlImGui.
/// </summary>
public static unsafe class ImGuiRlRendering
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ImDrawCallback(ImDrawList* parentList, ImDrawCmd* cmd);

    /// <summary>
    /// Renders an ImGui draw data list using Raylib's immediate-mode API.
    /// </summary>
    public static void RenderDrawData(ImDrawDataPtr drawData)
    {
        Rlgl.DrawRenderBatchActive();
        Rlgl.DisableBackfaceCulling();
        Rlgl.SetBlendMode(BlendMode.Alpha);
        Rlgl.DisableDepthTest();

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

                Rlgl.DrawRenderBatchActive();
            }
        }

        Rlgl.SetTexture(0);
        Rlgl.DisableScissorTest();
        Rlgl.EnableBackfaceCulling();
    }

    /// <summary>
    /// Processes the ImGui texture queue, creating/updating/destroying textures as needed.
    /// </summary>
    public static void ProcessTextures(Dictionary<int, Texture2D> managedTextures)
    {
        var platformIO = ImGui.GetPlatformIO();
        var textures = platformIO.Textures;

        for (int i = 0; i < textures.Size; i++)
        {
            var texData = textures.Data[i];
            var status = texData.Status;

            if (status == ImTextureStatus.WantCreate)
            {
                CreateTexture(managedTextures, texData);
            }
            else if (status == ImTextureStatus.WantUpdates)
            {
                UpdateTexture(managedTextures, texData);
            }
            else if (status == ImTextureStatus.WantDestroy)
            {
                DestroyTexture(managedTextures, texData);
            }
        }
    }

    /// <summary>
    /// Creates a GPU texture from ImGui texture data.
    /// </summary>
    public static void CreateTexture(Dictionary<int, Texture2D> managedTextures, ImTextureDataPtr texData)
    {
        int width = texData.Width;
        int height = texData.Height;
        int bytesPerPixel = texData.BytesPerPixel;
        byte* pixels = (byte*)texData.GetPixels();

        if (pixels == null || width <= 0 || height <= 0)
            return;

        byte* uploadPixels = pixels;
        bool needsFree = false;

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

        managedTextures[texData.UniqueID] = texture;
        texData.SetTexID(new ImTextureID((ulong)texture.Id));
        texData.SetStatus(ImTextureStatus.Ok);
    }

    /// <summary>
    /// Determines which channel in a multi-byte pixel format contains the alpha mask.
    /// </summary>
    public static int DetermineMaskChannel(byte* pixels, int pixelCount, int bytesPerPixel)
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

    /// <summary>
    /// Enables the scissor test for a clip rectangle.
    /// </summary>
    public static void EnableScissor(float x, float y, float width, float height)
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

    /// <summary>
    /// Processes gamepad input for ImGui.
    /// </summary>
    public static void ProcessGamepad(ImGuiIOPtr io)
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

    /// <summary>
    /// Populates a key map dictionary with Raylib-to-ImGui key mappings.
    /// </summary>
    public static void BuildKeyMap(Dictionary<KeyboardKey, ImGuiKey> keyMap)
    {
        keyMap.Clear();

        keyMap[KeyboardKey.A] = ImGuiKey.A;
        keyMap[KeyboardKey.B] = ImGuiKey.B;
        keyMap[KeyboardKey.C] = ImGuiKey.C;
        keyMap[KeyboardKey.D] = ImGuiKey.D;
        keyMap[KeyboardKey.E] = ImGuiKey.E;
        keyMap[KeyboardKey.F] = ImGuiKey.F;
        keyMap[KeyboardKey.G] = ImGuiKey.G;
        keyMap[KeyboardKey.H] = ImGuiKey.H;
        keyMap[KeyboardKey.I] = ImGuiKey.I;
        keyMap[KeyboardKey.J] = ImGuiKey.J;
        keyMap[KeyboardKey.K] = ImGuiKey.K;
        keyMap[KeyboardKey.L] = ImGuiKey.L;
        keyMap[KeyboardKey.M] = ImGuiKey.M;
        keyMap[KeyboardKey.N] = ImGuiKey.N;
        keyMap[KeyboardKey.O] = ImGuiKey.O;
        keyMap[KeyboardKey.P] = ImGuiKey.P;
        keyMap[KeyboardKey.Q] = ImGuiKey.Q;
        keyMap[KeyboardKey.R] = ImGuiKey.R;
        keyMap[KeyboardKey.S] = ImGuiKey.S;
        keyMap[KeyboardKey.T] = ImGuiKey.T;
        keyMap[KeyboardKey.U] = ImGuiKey.U;
        keyMap[KeyboardKey.V] = ImGuiKey.V;
        keyMap[KeyboardKey.W] = ImGuiKey.W;
        keyMap[KeyboardKey.X] = ImGuiKey.X;
        keyMap[KeyboardKey.Y] = ImGuiKey.Y;
        keyMap[KeyboardKey.Z] = ImGuiKey.Z;

        keyMap[KeyboardKey.Zero] = ImGuiKey.Key0;
        keyMap[KeyboardKey.One] = ImGuiKey.Key1;
        keyMap[KeyboardKey.Two] = ImGuiKey.Key2;
        keyMap[KeyboardKey.Three] = ImGuiKey.Key3;
        keyMap[KeyboardKey.Four] = ImGuiKey.Key4;
        keyMap[KeyboardKey.Five] = ImGuiKey.Key5;
        keyMap[KeyboardKey.Six] = ImGuiKey.Key6;
        keyMap[KeyboardKey.Seven] = ImGuiKey.Key7;
        keyMap[KeyboardKey.Eight] = ImGuiKey.Key8;
        keyMap[KeyboardKey.Nine] = ImGuiKey.Key9;

        keyMap[KeyboardKey.F1] = ImGuiKey.F1;
        keyMap[KeyboardKey.F2] = ImGuiKey.F2;
        keyMap[KeyboardKey.F3] = ImGuiKey.F3;
        keyMap[KeyboardKey.F4] = ImGuiKey.F4;
        keyMap[KeyboardKey.F5] = ImGuiKey.F5;
        keyMap[KeyboardKey.F6] = ImGuiKey.F6;
        keyMap[KeyboardKey.F7] = ImGuiKey.F7;
        keyMap[KeyboardKey.F8] = ImGuiKey.F8;
        keyMap[KeyboardKey.F9] = ImGuiKey.F9;
        keyMap[KeyboardKey.F10] = ImGuiKey.F10;
        keyMap[KeyboardKey.F11] = ImGuiKey.F11;
        keyMap[KeyboardKey.F12] = ImGuiKey.F12;

        keyMap[KeyboardKey.Left] = ImGuiKey.LeftArrow;
        keyMap[KeyboardKey.Right] = ImGuiKey.RightArrow;
        keyMap[KeyboardKey.Up] = ImGuiKey.UpArrow;
        keyMap[KeyboardKey.Down] = ImGuiKey.DownArrow;
        keyMap[KeyboardKey.PageUp] = ImGuiKey.PageUp;
        keyMap[KeyboardKey.PageDown] = ImGuiKey.PageDown;
        keyMap[KeyboardKey.Home] = ImGuiKey.Home;
        keyMap[KeyboardKey.End] = ImGuiKey.End;
        keyMap[KeyboardKey.Insert] = ImGuiKey.Insert;
        keyMap[KeyboardKey.Delete] = ImGuiKey.Delete;
        keyMap[KeyboardKey.Backspace] = ImGuiKey.Backspace;
        keyMap[KeyboardKey.Enter] = ImGuiKey.Enter;
        keyMap[KeyboardKey.Escape] = ImGuiKey.Escape;
        keyMap[KeyboardKey.Tab] = ImGuiKey.Tab;
        keyMap[KeyboardKey.Space] = ImGuiKey.Space;

        keyMap[KeyboardKey.LeftShift] = ImGuiKey.LeftShift;
        keyMap[KeyboardKey.RightShift] = ImGuiKey.RightShift;
        keyMap[KeyboardKey.LeftControl] = ImGuiKey.LeftCtrl;
        keyMap[KeyboardKey.RightControl] = ImGuiKey.RightCtrl;
        keyMap[KeyboardKey.LeftAlt] = ImGuiKey.LeftAlt;
        keyMap[KeyboardKey.RightAlt] = ImGuiKey.RightAlt;
        keyMap[KeyboardKey.LeftSuper] = ImGuiKey.LeftSuper;
        keyMap[KeyboardKey.RightSuper] = ImGuiKey.RightSuper;

        keyMap[KeyboardKey.Apostrophe] = ImGuiKey.Apostrophe;
        keyMap[KeyboardKey.Comma] = ImGuiKey.Comma;
        keyMap[KeyboardKey.Minus] = ImGuiKey.Minus;
        keyMap[KeyboardKey.Period] = ImGuiKey.Period;
        keyMap[KeyboardKey.Slash] = ImGuiKey.Slash;
        keyMap[KeyboardKey.Semicolon] = ImGuiKey.Semicolon;
        keyMap[KeyboardKey.Equal] = ImGuiKey.Equal;
        keyMap[KeyboardKey.LeftBracket] = ImGuiKey.LeftBracket;
        keyMap[KeyboardKey.Backslash] = ImGuiKey.Backslash;
        keyMap[KeyboardKey.RightBracket] = ImGuiKey.RightBracket;
        keyMap[KeyboardKey.Grave] = ImGuiKey.GraveAccent;

        keyMap[KeyboardKey.CapsLock] = ImGuiKey.CapsLock;
        keyMap[KeyboardKey.ScrollLock] = ImGuiKey.ScrollLock;
        keyMap[KeyboardKey.NumLock] = ImGuiKey.NumLock;
        keyMap[KeyboardKey.PrintScreen] = ImGuiKey.PrintScreen;
        keyMap[KeyboardKey.Pause] = ImGuiKey.Pause;

        keyMap[KeyboardKey.Kp0] = ImGuiKey.Keypad0;
        keyMap[KeyboardKey.Kp1] = ImGuiKey.Keypad1;
        keyMap[KeyboardKey.Kp2] = ImGuiKey.Keypad2;
        keyMap[KeyboardKey.Kp3] = ImGuiKey.Keypad3;
        keyMap[KeyboardKey.Kp4] = ImGuiKey.Keypad4;
        keyMap[KeyboardKey.Kp5] = ImGuiKey.Keypad5;
        keyMap[KeyboardKey.Kp6] = ImGuiKey.Keypad6;
        keyMap[KeyboardKey.Kp7] = ImGuiKey.Keypad7;
        keyMap[KeyboardKey.Kp8] = ImGuiKey.Keypad8;
        keyMap[KeyboardKey.Kp9] = ImGuiKey.Keypad9;
        keyMap[KeyboardKey.KpDecimal] = ImGuiKey.KeypadDecimal;
        keyMap[KeyboardKey.KpDivide] = ImGuiKey.KeypadDivide;
        keyMap[KeyboardKey.KpMultiply] = ImGuiKey.KeypadMultiply;
        keyMap[KeyboardKey.KpSubtract] = ImGuiKey.KeypadSubtract;
        keyMap[KeyboardKey.KpAdd] = ImGuiKey.KeypadAdd;
        keyMap[KeyboardKey.KpEnter] = ImGuiKey.KeypadEnter;
        keyMap[KeyboardKey.KpEqual] = ImGuiKey.KeypadEqual;
    }

    /// <summary>
    /// Populates a cursor map dictionary with ImGui-to-Raylib cursor mappings.
    /// </summary>
    public static void BuildCursorMap(Dictionary<ImGuiMouseCursor, MouseCursor> cursorMap)
    {
        cursorMap.Clear();
        cursorMap[ImGuiMouseCursor.Arrow] = MouseCursor.Arrow;
        cursorMap[ImGuiMouseCursor.TextInput] = MouseCursor.IBeam;
        cursorMap[ImGuiMouseCursor.ResizeAll] = MouseCursor.ResizeAll;
        cursorMap[ImGuiMouseCursor.ResizeNs] = MouseCursor.ResizeNs;
        cursorMap[ImGuiMouseCursor.ResizeEw] = MouseCursor.ResizeEw;
        cursorMap[ImGuiMouseCursor.ResizeNesw] = MouseCursor.ResizeNesw;
        cursorMap[ImGuiMouseCursor.ResizeNwse] = MouseCursor.ResizeNwse;
        cursorMap[ImGuiMouseCursor.Hand] = MouseCursor.PointingHand;
        cursorMap[ImGuiMouseCursor.NotAllowed] = MouseCursor.NotAllowed;
    }

    private static void UpdateTexture(Dictionary<int, Texture2D> managedTextures, ImTextureDataPtr texData)
    {
        if (managedTextures.TryGetValue(texData.UniqueID, out var existing))
            Raylib.UnloadTexture(existing);

        CreateTexture(managedTextures, texData);
    }

    private static void DestroyTexture(Dictionary<int, Texture2D> managedTextures, ImTextureDataPtr texData)
    {
        if (managedTextures.TryGetValue(texData.UniqueID, out var texture))
        {
            Raylib.UnloadTexture(texture);
            managedTextures.Remove(texData.UniqueID);
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
}
