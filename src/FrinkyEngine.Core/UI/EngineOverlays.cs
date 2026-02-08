using System.Numerics;
using Hexa.NET.ImGui;
using Raylib_cs;

namespace FrinkyEngine.Core.UI;

/// <summary>
/// Built-in engine overlays (FPS counter, developer console) that render through the Game UI pipeline.
/// </summary>
public static class EngineOverlays
{
    private const int ConsoleHistoryMax = 128;
    private const float FpsRefreshInterval = 0.25f;

    private static bool _fpsVisible;
    private static int _displayedFps;
    private static float _displayedFrameTime;
    private static float _fpsAccumulator;

    private static bool _consoleVisible;
    private static string _consoleInput = string.Empty;
    private static readonly List<string> ConsoleHistory = new();
    private static bool _consoleFocusInput;

    /// <summary>
    /// Checks keybinds and queues overlay draw commands for the current frame.
    /// Call once per frame after scene update.
    /// </summary>
    /// <param name="dt">Frame delta time in seconds.</param>
    public static void Update(float dt)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.F3))
            _fpsVisible = !_fpsVisible;

        if (Raylib.IsKeyPressed(KeyboardKey.Grave))
        {
            _consoleVisible = !_consoleVisible;
            if (_consoleVisible)
                _consoleFocusInput = true;
        }

        if (_fpsVisible)
        {
            _fpsAccumulator += dt;
            if (_fpsAccumulator >= FpsRefreshInterval)
            {
                _fpsAccumulator = 0f;
                _displayedFps = Raylib.GetFPS();
                _displayedFrameTime = Raylib.GetFrameTime() * 1000f;
            }

            UI.Draw(_ => DrawFpsCounter());
        }

        if (_consoleVisible)
            UI.Draw(_ => DrawConsole());
    }

    /// <summary>
    /// Resets console state (history and input). Called when exiting editor play mode.
    /// FPS toggle intentionally persists.
    /// </summary>
    public static void Reset()
    {
        _consoleVisible = false;
        _consoleInput = string.Empty;
        ConsoleHistory.Clear();
        _consoleFocusInput = false;
    }

    private static void DrawFpsCounter()
    {
        var io = ImGui.GetIO();
        var pos = new Vector2(io.DisplaySize.X - 10f, 10f);
        ImGui.SetNextWindowPos(pos, ImGuiCond.Always, new Vector2(1f, 0f));
        ImGui.SetNextWindowBgAlpha(0.35f);

        var flags = ImGuiWindowFlags.NoDecoration
                    | ImGuiWindowFlags.AlwaysAutoResize
                    | ImGuiWindowFlags.NoFocusOnAppearing
                    | ImGuiWindowFlags.NoNav
                    | ImGuiWindowFlags.NoMove
                    | ImGuiWindowFlags.NoSavedSettings;

        if (ImGui.Begin("##EngineOverlay_FPS", flags))
        {
            var green = new Vector4(0.2f, 1.0f, 0.2f, 1.0f);
            ImGui.PushStyleColor(ImGuiCol.Text, green);
            ImGui.Text($"FPS: {_displayedFps}");
            ImGui.Text($"Frame: {_displayedFrameTime:F1}ms");
            ImGui.PopStyleColor();
        }
        ImGui.End();
    }

    private static void DrawConsole()
    {
        var io = ImGui.GetIO();
        float width = io.DisplaySize.X;
        float height = io.DisplaySize.Y * 0.4f;

        ImGui.SetNextWindowPos(Vector2.Zero, ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(width, height), ImGuiCond.Always);
        ImGui.SetNextWindowBgAlpha(0.85f);

        var flags = ImGuiWindowFlags.NoDecoration
                    | ImGuiWindowFlags.NoMove
                    | ImGuiWindowFlags.NoResize
                    | ImGuiWindowFlags.NoSavedSettings
                    | ImGuiWindowFlags.NoFocusOnAppearing
                    | ImGuiWindowFlags.NoNav;

        if (ImGui.Begin("##EngineOverlay_Console", flags))
        {
            float inputHeight = ImGui.GetFrameHeightWithSpacing();
            float outputHeight = height - inputHeight - ImGui.GetCursorPosY() - 8f;

            if (ImGui.BeginChild("##ConsoleOutput", new Vector2(0, outputHeight), ImGuiChildFlags.None, ImGuiWindowFlags.None))
            {
                foreach (var line in ConsoleHistory)
                    ImGui.TextUnformatted(line);

                if (ImGui.GetScrollY() >= ImGui.GetScrollMaxY() - 4f)
                    ImGui.SetScrollHereY(1.0f);
            }
            ImGui.EndChild();

            ImGui.Separator();

            ImGui.PushItemWidth(-1);
            bool windowAppearing = ImGui.IsWindowAppearing();
            if (_consoleFocusInput && !windowAppearing)
            {
                ImGui.SetKeyboardFocusHere();
                _consoleFocusInput = false;
            }

            if (ImGui.InputTextWithHint("##ConsoleInput", "Enter command...", ref _consoleInput, 512,
                    ImGuiInputTextFlags.EnterReturnsTrue))
            {
                var trimmed = _consoleInput.Trim();
                if (trimmed.Length > 0)
                {
                    AddHistoryLine($"> {trimmed}");
                    AddHistoryLine("[No commands available]");
                }
                _consoleInput = string.Empty;
                _consoleFocusInput = true;
            }
            ImGui.PopItemWidth();
        }
        ImGui.End();
    }

    private static void AddHistoryLine(string line)
    {
        ConsoleHistory.Add(line);
        while (ConsoleHistory.Count > ConsoleHistoryMax)
            ConsoleHistory.RemoveAt(0);
    }
}
