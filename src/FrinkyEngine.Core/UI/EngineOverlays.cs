using System.Numerics;
using System.Runtime.InteropServices;
using FrinkyEngine.Core.Audio;
using FrinkyEngine.Core.Physics;
using FrinkyEngine.Core.Rendering;
using FrinkyEngine.Core.Rendering.Profiling;
using FrinkyEngine.Core.Scene;
using FrinkyEngine.Core.UI.ConsoleSystem;
using Hexa.NET.ImGui;
using Raylib_cs;

namespace FrinkyEngine.Core.UI;

/// <summary>
/// Built-in engine overlays (stats overlay, developer console) that render through the Game UI pipeline.
/// </summary>
public static unsafe class EngineOverlays
{
    private enum StatsOverlayMode : byte
    {
        None = 0,
        FpsAndMs = 1,
        Advanced = 2,
        Verbose = 3
    }

    private const int ConsoleHistoryMax = 128;
    private const int ConsoleCommandHistoryMax = 128;
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
    private static readonly List<string> _consoleCommandHistory = new();
    private static int _consoleHistoryCursor = -1;
    private static string _consoleHistoryDraft = string.Empty;
    private static readonly List<string> _autocompleteMatches = new();
    private static string _autocompleteLeading = string.Empty;
    private static string _autocompleteSuffix = string.Empty;
    private static int _autocompleteMatchIndex = -1;
    private static readonly ImGuiInputTextCallback ConsoleInputCallback = HandleConsoleInputTextCallback;
    private static bool _consoleFocusInput;
    private static bool _consoleBackendInitialized;

    /// <summary>
    /// Gets whether the developer console overlay is currently visible.
    /// </summary>
    public static bool IsConsoleVisible => _consoleVisible;

    /// <summary>
    /// Checks keybinds and queues overlay draw commands for the current frame.
    /// Call once per frame after scene update.
    /// </summary>
    /// <param name="dt">Frame delta time in seconds.</param>
    public static void Update(float dt)
    {
        EnsureConsoleBackendInitialized();

        if (Raylib.IsKeyPressed(KeyboardKey.F3))
            _statsMode = (StatsOverlayMode)(((int)_statsMode + 1) % 4);

        if (Raylib.IsKeyPressed(KeyboardKey.Grave))
        {
            _consoleVisible = !_consoleVisible;
            if (_consoleVisible)
            {
                FrinkyLog.Info("Developer console opened.");
                _consoleFocusInput = true;
            }
            else
            {
                FrinkyLog.Info("Developer console closed.");
                ClearInputNavigationState(restoreDraft: false);
                _consoleInput = string.Empty;
                _consoleFocusInput = false;
            }
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
        _consoleCommandHistory.Clear();
        _consoleHistoryCursor = -1;
        _consoleHistoryDraft = string.Empty;
        ResetAutocompleteState();
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
                    ImGuiInputTextFlags.EnterReturnsTrue
                    | ImGuiInputTextFlags.CallbackCompletion
                    | ImGuiInputTextFlags.CallbackHistory,
                    ConsoleInputCallback))
            {
                SubmitConsoleInput();
            }
            else if (ImGui.IsItemEdited())
            {
                if (_consoleHistoryCursor != -1)
                {
                    _consoleHistoryCursor = -1;
                    _consoleHistoryDraft = _consoleInput;
                }

                ResetAutocompleteState();
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

    private static void SubmitConsoleInput()
    {
        var trimmed = _consoleInput.Trim();
        if (trimmed.Length > 0)
        {
            AddCommandHistoryEntry(trimmed);
            AddHistoryLine($"> {trimmed}");

            var result = ConsoleBackend.Execute(trimmed);
            foreach (var line in result.Lines)
                AddHistoryLine(line);
        }

        _consoleInput = string.Empty;
        ClearInputNavigationState(restoreDraft: false);
        _consoleFocusInput = true;
    }

    private static void AddCommandHistoryEntry(string command)
    {
        if (_consoleCommandHistory.Count > 0
            && string.Equals(_consoleCommandHistory[^1], command, StringComparison.Ordinal))
        {
            return;
        }

        _consoleCommandHistory.Add(command);
        while (_consoleCommandHistory.Count > ConsoleCommandHistoryMax)
            _consoleCommandHistory.RemoveAt(0);
    }

    private static void ResetAutocompleteState()
    {
        _autocompleteMatches.Clear();
        _autocompleteLeading = string.Empty;
        _autocompleteSuffix = string.Empty;
        _autocompleteMatchIndex = -1;
    }

    private static void ClearInputNavigationState(bool restoreDraft)
    {
        if (restoreDraft && _consoleHistoryCursor != -1)
            _consoleInput = _consoleHistoryDraft;

        _consoleHistoryCursor = -1;
        _consoleHistoryDraft = string.Empty;
        ResetAutocompleteState();
    }

    private static unsafe int HandleConsoleInputTextCallback(ImGuiInputTextCallbackData* data)
    {
        if (data == null)
            return 0;

        if (data->EventFlag == ImGuiInputTextFlags.CallbackCompletion)
        {
            FrinkyLog.Info("Console callback: completion (Tab).");
            ApplyAutocompleteCycle(data);
        }
        else if (data->EventFlag == ImGuiInputTextFlags.CallbackHistory)
        {
            if (data->EventKey == ImGuiKey.UpArrow)
            {
                FrinkyLog.Info("Console callback: history Up.");
                NavigateHistoryUp(data);
            }
            else if (data->EventKey == ImGuiKey.DownArrow)
            {
                FrinkyLog.Info("Console callback: history Down.");
                NavigateHistoryDown(data);
            }
            else
            {
                FrinkyLog.Info($"Console callback: history key ignored ({data->EventKey}).");
            }
        }

        return 0;
    }

    private static unsafe void NavigateHistoryUp(ImGuiInputTextCallbackData* data)
    {
        if (_consoleCommandHistory.Count == 0 || data == null)
            return;

        var current = GetCallbackBuffer(data);

        if (_consoleHistoryCursor == -1)
        {
            _consoleHistoryDraft = current;
            _consoleHistoryCursor = _consoleCommandHistory.Count - 1;
        }
        else if (_consoleHistoryCursor > 0)
        {
            _consoleHistoryCursor--;
        }

        var next = _consoleCommandHistory[_consoleHistoryCursor];
        SetCallbackBuffer(data, next);
        _consoleInput = next;
        ResetAutocompleteState();
        FrinkyLog.Info($"Console history cursor -> {_consoleHistoryCursor}.");
    }

    private static unsafe void NavigateHistoryDown(ImGuiInputTextCallbackData* data)
    {
        if (_consoleHistoryCursor == -1 || data == null)
            return;

        string next;
        if (_consoleHistoryCursor < _consoleCommandHistory.Count - 1)
        {
            _consoleHistoryCursor++;
            next = _consoleCommandHistory[_consoleHistoryCursor];
        }
        else
        {
            _consoleHistoryCursor = -1;
            next = _consoleHistoryDraft;
        }

        SetCallbackBuffer(data, next);
        _consoleInput = next;
        ResetAutocompleteState();
        FrinkyLog.Info($"Console history cursor -> {_consoleHistoryCursor}.");
    }

    private static unsafe void ApplyAutocompleteCycle(ImGuiInputTextCallbackData* data)
    {
        if (data == null)
            return;

        var current = GetCallbackBuffer(data);
        SplitFirstToken(current, out var leading, out var token, out var suffix);

        var continuingCycle = _autocompleteMatchIndex >= 0
                              && _autocompleteMatchIndex < _autocompleteMatches.Count
                              && string.Equals(leading, _autocompleteLeading, StringComparison.Ordinal)
                              && string.Equals(suffix, _autocompleteSuffix, StringComparison.Ordinal)
                              && string.Equals(token, _autocompleteMatches[_autocompleteMatchIndex], StringComparison.OrdinalIgnoreCase);

        if (!continuingCycle)
        {
            ResetAutocompleteState();

            var matches = ConsoleBackend.GetRegisteredNames()
                .Where(name => name.StartsWith(token, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (matches.Length == 0)
            {
                FrinkyLog.Info($"Console autocomplete: no matches for '{token}'.");
                return;
            }

            _autocompleteLeading = leading;
            _autocompleteSuffix = suffix;
            _autocompleteMatches.AddRange(matches);
            _autocompleteMatchIndex = -1;
        }

        _autocompleteMatchIndex = (_autocompleteMatchIndex + 1) % _autocompleteMatches.Count;
        var next = string.Concat(_autocompleteLeading, _autocompleteMatches[_autocompleteMatchIndex], _autocompleteSuffix);
        SetCallbackBuffer(data, next);
        _consoleInput = next;
        _consoleHistoryCursor = -1;
        _consoleHistoryDraft = _consoleInput;
        FrinkyLog.Info($"Console autocomplete -> '{_autocompleteMatches[_autocompleteMatchIndex]}'.");
    }

    private static unsafe string GetCallbackBuffer(ImGuiInputTextCallbackData* data)
    {
        if (data == null || data->Buf == null || data->BufTextLen <= 0)
            return string.Empty;

        return Marshal.PtrToStringUTF8((IntPtr)data->Buf, data->BufTextLen) ?? string.Empty;
    }

    private static unsafe void SetCallbackBuffer(ImGuiInputTextCallbackData* data, string text)
    {
        if (data == null)
            return;

        text ??= string.Empty;

        int maxLen = Math.Max(0, data->BufSize - 1);
        if (text.Length > maxLen)
            text = text[..maxLen];

        data->DeleteChars(0, data->BufTextLen);
        if (text.Length > 0)
            data->InsertChars(0, text);

        data->BufDirty = 1;
        data->CursorPos = text.Length;
        data->SelectionStart = text.Length;
        data->SelectionEnd = text.Length;
    }

    private static void SplitFirstToken(string input, out string leading, out string token, out string suffix)
    {
        input ??= string.Empty;

        int tokenStart = 0;
        while (tokenStart < input.Length && char.IsWhiteSpace(input[tokenStart]))
            tokenStart++;

        leading = tokenStart > 0 ? input[..tokenStart] : string.Empty;
        if (tokenStart >= input.Length)
        {
            token = string.Empty;
            suffix = string.Empty;
            return;
        }

        int tokenEnd = tokenStart;
        while (tokenEnd < input.Length && !char.IsWhiteSpace(input[tokenEnd]))
            tokenEnd++;

        token = input[tokenStart..tokenEnd];
        suffix = tokenEnd < input.Length ? input[tokenEnd..] : string.Empty;
    }

    private static void EnsureConsoleBackendInitialized()
    {
        if (_consoleBackendInitialized)
            return;

        ConsoleBackend.EnsureBuiltinsRegistered();
        ConsoleBackend.RegisterCVar(new ConsoleCVar(
            "r_postprocess",
            "r_postprocess [0|1]",
            "Enable or disable post-processing in runtime and Play/Simulate (1=on, 0=off).",
            RenderRuntimeCvars.GetPostProcessingValue,
            RenderRuntimeCvars.TrySetPostProcessing));

        _consoleBackendInitialized = true;
    }
}
