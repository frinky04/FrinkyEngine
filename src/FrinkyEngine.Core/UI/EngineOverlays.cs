using System.Numerics;
using FrinkyEngine.Core.Audio;
using FrinkyEngine.Core.Physics;
using FrinkyEngine.Core.Rendering.Profiling;
using FrinkyEngine.Core.Scene;
using Hexa.NET.ImGui;
using Raylib_cs;

namespace FrinkyEngine.Core.UI;

/// <summary>
/// Built-in engine overlays (stats overlay, developer console) that render through the Game UI pipeline.
/// </summary>
public static class EngineOverlays
{
    private enum StatsOverlayMode : byte
    {
        None = 0,
        FpsAndMs = 1,
        Advanced = 2,
        Verbose = 3
    }

    private const int ConsoleHistoryMax = 128;
    private const int VerboseMaxSubTimings = 5;
    private const float FpsRefreshInterval = 0.1f;

    private static StatsOverlayMode _statsMode;
    private static int _displayedFps;
    private static float _displayedFrameTime;
    private static int _displayedScreenWidth;
    private static int _displayedScreenHeight;
    private static FrameSnapshot _displayedSnapshot;
    private static bool _displayedSnapshotValid;
    private static int _displayedEntityCount;
    private static PhysicsFrameStats _displayedPhysicsStats;
    private static AudioFrameStats _displayedAudioStats;
    private static SubCategoryTiming[] _displayedSubTimings = Array.Empty<SubCategoryTiming>();
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
            _statsMode = (StatsOverlayMode)(((int)_statsMode + 1) % 4);

        if (Raylib.IsKeyPressed(KeyboardKey.Grave))
        {
            _consoleVisible = !_consoleVisible;
            if (_consoleVisible)
                _consoleFocusInput = true;
        }

        if (_statsMode != StatsOverlayMode.None)
        {
            _fpsAccumulator += dt;
            if (_fpsAccumulator >= FpsRefreshInterval)
            {
                _fpsAccumulator = 0f;
                RefreshDisplayedStats();
            }

            UI.Draw(_ => DrawStatsOverlay());
        }

        if (_consoleVisible)
            UI.Draw(_ => DrawConsole());
    }

    /// <summary>
    /// Resets console state (history and input). Called when exiting editor play mode.
    /// Stats overlay mode intentionally persists.
    /// </summary>
    public static void Reset()
    {
        _consoleVisible = false;
        _consoleInput = string.Empty;
        ConsoleHistory.Clear();
        _consoleFocusInput = false;
    }

    private static void RefreshDisplayedStats()
    {
        _displayedFps = Raylib.GetFPS();
        _displayedFrameTime = Raylib.GetFrameTime() * 1000f;
        _displayedScreenWidth = Raylib.GetScreenWidth();
        _displayedScreenHeight = Raylib.GetScreenHeight();

        var scene = SceneManager.Instance.ActiveScene;
        _displayedEntityCount = scene?.Entities.Count ?? 0;
        _displayedPhysicsStats = scene?.GetPhysicsFrameStats() ?? default;
        _displayedAudioStats = scene?.GetAudioFrameStats() ?? default;

        _displayedSnapshotValid = FrameProfiler.Enabled && FrameProfiler.FrameCount > 0;
        if (!_displayedSnapshotValid)
        {
            _displayedSnapshot = default;
            _displayedSubTimings = Array.Empty<SubCategoryTiming>();
            return;
        }

        _displayedSnapshot = FrameProfiler.GetLatest();
        _displayedSubTimings = _displayedSnapshot.SubTimings ?? Array.Empty<SubCategoryTiming>();
    }

    private static void DrawStatsOverlay()
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

        if (ImGui.Begin("##EngineOverlay_Stats", flags))
        {
            var green = new Vector4(0.2f, 1.0f, 0.2f, 1.0f);
            ImGui.PushStyleColor(ImGuiCol.Text, green);
            ImGui.Text($"FPS: {_displayedFps}");
            ImGui.Text($"Frame: {_displayedFrameTime:F1}ms");

            if (_statsMode >= StatsOverlayMode.Advanced)
                DrawAdvancedStats();

            if (_statsMode >= StatsOverlayMode.Verbose)
                DrawVerboseStats();

            ImGui.PopStyleColor();
        }
        ImGui.End();
    }

    private static void DrawAdvancedStats()
    {
        ImGui.Separator();

        if (!FrameProfiler.Enabled)
        {
            ImGui.TextDisabled("Profiler: disabled");
            return;
        }

        if (FrameProfiler.FrameCount == 0)
        {
            ImGui.TextDisabled("Profiler: no data yet");
            return;
        }

        if (!_displayedSnapshotValid)
        {
            ImGui.TextDisabled("Profiler: waiting for sample");
            return;
        }

        var latest = _displayedSnapshot;

        ImGui.Text($"CPU: {latest.TotalFrameMs:F2}ms");
        ImGui.Text($"Game: {latest.GetCategoryMs(ProfileCategory.Game):F2}  Late: {latest.GetCategoryMs(ProfileCategory.GameLate):F2}");
        ImGui.Text($"Phys: {latest.GetCategoryMs(ProfileCategory.Physics):F2}  Audio: {latest.GetCategoryMs(ProfileCategory.Audio):F2}");
        ImGui.Text($"Render: {latest.GetCategoryMs(ProfileCategory.Rendering):F2}  Post: {latest.GetCategoryMs(ProfileCategory.PostProcessing):F2}");
        ImGui.Text($"UI: {latest.GetCategoryMs(ProfileCategory.UI):F2}  Other: {latest.OtherMs:F2}");

        var editorMs = latest.GetCategoryMs(ProfileCategory.Editor);
        if (editorMs > 0.001)
            ImGui.Text($"Editor: {editorMs:F2}");

        ImGui.Text($"Resolution: {_displayedScreenWidth}x{_displayedScreenHeight}");
        ImGui.Text($"Entities: {_displayedEntityCount}  PP Passes: {latest.GpuStats.PostProcessPasses}");
    }

    private static void DrawVerboseStats()
    {
        if (!FrameProfiler.Enabled || FrameProfiler.FrameCount == 0)
            return;

        if (!_displayedSnapshotValid)
            return;

        var latest = _displayedSnapshot;

        ImGui.Separator();

        if (_displayedPhysicsStats.Valid)
        {
            ImGui.Text("Physics");
            ImGui.Text($"Bodies D/K/S: {_displayedPhysicsStats.DynamicBodies}/{_displayedPhysicsStats.KinematicBodies}/{_displayedPhysicsStats.StaticBodies}");
            ImGui.Text($"Substeps: {_displayedPhysicsStats.SubstepsThisFrame}  Step: {_displayedPhysicsStats.StepTimeMs:F2}ms  CC: {_displayedPhysicsStats.ActiveCharacterControllers}");
        }

        if (_displayedAudioStats.Valid)
        {
            ImGui.Text("Audio");
            ImGui.Text($"Voices: {_displayedAudioStats.ActiveVoices}  Streaming: {_displayedAudioStats.StreamingVoices}");
            ImGui.Text($"Virtual: {_displayedAudioStats.VirtualizedVoices}  Stolen: {_displayedAudioStats.StolenVoicesThisFrame}  Update: {_displayedAudioStats.UpdateTimeMs:F2}ms");
        }

        var subTimings = _displayedSubTimings;
        if (subTimings == null || subTimings.Length == 0)
            return;

        ImGui.Text("Post FX (Top)");

        var used = new bool[subTimings.Length];
        int shown = 0;
        int max = Math.Min(VerboseMaxSubTimings, subTimings.Length);
        while (shown < max)
        {
            int bestIndex = -1;
            double bestMs = double.MinValue;
            for (int i = 0; i < subTimings.Length; i++)
            {
                if (used[i])
                    continue;

                if (subTimings[i].ElapsedMs > bestMs)
                {
                    bestMs = subTimings[i].ElapsedMs;
                    bestIndex = i;
                }
            }

            if (bestIndex < 0 || bestMs <= 0)
                break;

            used[bestIndex] = true;
            var sub = subTimings[bestIndex];
            ImGui.Text($"{sub.Name}: {sub.ElapsedMs:F3}ms");
            shown++;
        }
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
