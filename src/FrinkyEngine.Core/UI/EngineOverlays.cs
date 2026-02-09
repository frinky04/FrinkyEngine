using System.Numerics;
using System.Runtime.InteropServices;
using FrinkyEngine.Core.Audio;
using AudioApi = FrinkyEngine.Core.Audio.Audio;
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

    private readonly struct ConsoleSuggestionItem(ConsoleEntryDescriptor entry, int score)
    {
        public ConsoleEntryDescriptor Entry { get; } = entry;
        public int Score { get; } = score;
    }

    private const int ConsoleHistoryMax = 128;
    private const int ConsoleCommandHistoryMax = 128;
    private const int ConsoleSuggestionVisibleRows = 5;
    private const int ConsoleSuggestionMaxMatches = 32;
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
    private static double _displayedIdleMs;
    private static float _fpsAccumulator;

    private static bool _consoleVisible;
    private static string _consoleInput = string.Empty;
    private static readonly List<string> ConsoleHistory = new();
    private static readonly List<string> _consoleCommandHistory = new();
    private static int _consoleHistoryCursor = -1;
    private static string _consoleHistoryDraft = string.Empty;
    private static readonly List<ConsoleSuggestionItem> _consoleSuggestions = new();
    private static int _consoleSuggestionIndex = -1;
    private static readonly ImGuiInputTextCallback ConsoleInputCallback = HandleConsoleInputTextCallback;
    private static bool _consoleFocusInput;
    private static bool _consoleAutoScroll = true;
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
        _consoleAutoScroll = true;
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
        _displayedIdleMs = FrameProfiler.GetLatestIdleMs();
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

        bool isUncapped = RenderRuntimeCvars.TargetFps == 0
                          && !Raylib.IsWindowState(ConfigFlags.VSyncHint);
        if (isUncapped)
            ImGui.Text($"GPU (est): {_displayedIdleMs:F2}ms");
        else
            ImGui.Text($"Idle: {_displayedIdleMs:F2}ms");

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
            outputHeight = Math.Max(1f, outputHeight);

            if (ImGui.BeginChild("##ConsoleOutput", new Vector2(0, outputHeight), ImGuiChildFlags.None, ImGuiWindowFlags.None))
            {
                foreach (var line in ConsoleHistory)
                    ImGui.TextUnformatted(line);

                bool isAtBottom = ImGui.GetScrollY() >= ImGui.GetScrollMaxY() - 4f;

                if (isAtBottom)
                    _consoleAutoScroll = true;

                if (ImGui.IsWindowHovered() && ImGui.GetIO().MouseWheel != 0 && !isAtBottom)
                    _consoleAutoScroll = false;

                if (_consoleAutoScroll)
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
                    | ImGuiInputTextFlags.CallbackHistory
                    | ImGuiInputTextFlags.CallbackEdit,
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

            var inputRectMin = ImGui.GetItemRectMin();
            var inputRectMax = ImGui.GetItemRectMax();
            RefreshSuggestions(_consoleInput);
            ImGui.PopItemWidth();
            DrawSuggestionPanel(inputRectMin, inputRectMax);
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
        TryApplyActiveSuggestionToInput(advanceSelection: false);

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
        _consoleAutoScroll = true;
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
        _consoleSuggestions.Clear();
        _consoleSuggestionIndex = -1;
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
            ApplyAutocompleteCycle(data);
        }
        else if (data->EventFlag == ImGuiInputTextFlags.CallbackEdit)
        {
            _consoleInput = GetCallbackBuffer(data);
        }
        else if (data->EventFlag == ImGuiInputTextFlags.CallbackHistory)
        {
            if (_consoleSuggestions.Count > 0)
            {
                if (data->EventKey == ImGuiKey.UpArrow)
                {
                    if (_consoleSuggestionIndex > 0)
                        _consoleSuggestionIndex--;
                    else
                        _consoleSuggestionIndex = _consoleSuggestions.Count - 1;
                }
                else if (data->EventKey == ImGuiKey.DownArrow)
                {
                    if (_consoleSuggestionIndex < _consoleSuggestions.Count - 1)
                        _consoleSuggestionIndex++;
                    else
                        _consoleSuggestionIndex = 0;
                }
            }
            else
            {
                if (data->EventKey == ImGuiKey.UpArrow)
                    NavigateHistoryUp(data);
                else if (data->EventKey == ImGuiKey.DownArrow)
                    NavigateHistoryDown(data);
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
    }

    private static unsafe void ApplyAutocompleteCycle(ImGuiInputTextCallbackData* data)
    {
        if (data == null)
            return;

        var current = GetCallbackBuffer(data);
        TryApplyActiveSuggestionToBuffer(data, current, advanceSelection: true);
    }

    private static bool TryApplyActiveSuggestionToInput(bool advanceSelection)
    {
        if (!TryApplySuggestionToText(_consoleInput, advanceSelection, out var replaced))
            return false;

        _consoleInput = replaced;
        _consoleHistoryCursor = -1;
        _consoleHistoryDraft = _consoleInput;
        return true;
    }

    private static unsafe bool TryApplyActiveSuggestionToBuffer(
        ImGuiInputTextCallbackData* data,
        string currentInput,
        bool advanceSelection)
    {
        if (data == null)
            return false;

        if (!TryApplySuggestionToText(currentInput, advanceSelection, out var replaced))
            return false;

        SetCallbackBuffer(data, replaced);
        _consoleInput = replaced;
        _consoleHistoryCursor = -1;
        _consoleHistoryDraft = _consoleInput;
        return true;
    }

    private static bool TryApplySuggestionToText(string input, bool advanceSelection, out string replaced)
    {
        replaced = input ?? string.Empty;
        RefreshSuggestions(replaced);

        if (_consoleSuggestions.Count == 0)
            return false;

        if (_consoleSuggestionIndex < 0 || _consoleSuggestionIndex >= _consoleSuggestions.Count)
            _consoleSuggestionIndex = 0;

        replaced = ReplaceFirstToken(replaced, _consoleSuggestions[_consoleSuggestionIndex].Entry.Name);

        if (advanceSelection)
            _consoleSuggestionIndex = (_consoleSuggestionIndex + 1) % _consoleSuggestions.Count;
        return true;
    }

    private static float CalculateSuggestionPanelHeight()
    {
        if (_consoleSuggestions.Count == 0)
            return 0f;

        int rows = Math.Min(ConsoleSuggestionVisibleRows, _consoleSuggestions.Count);
        float windowPadding = ImGui.GetStyle().WindowPadding.Y * 2;
        return rows * ImGui.GetTextLineHeightWithSpacing() + windowPadding + 4f;
    }

    private static void DrawSuggestionPanel(Vector2 inputRectMin, Vector2 inputRectMax)
    {
        if (_consoleSuggestions.Count == 0)
            return;

        float panelHeight = CalculateSuggestionPanelHeight();
        float panelWidth = Math.Max(160f, inputRectMax.X - inputRectMin.X);
        float panelX = inputRectMin.X;
        float panelY = inputRectMax.Y + 4f;
        float screenHeight = ImGui.GetIO().DisplaySize.Y;
        if (panelY + panelHeight > screenHeight)
        {
            float aboveY = inputRectMin.Y - panelHeight - 4f;
            if (aboveY >= 0f)
                panelY = aboveY;
            else
                panelY = Math.Max(0f, screenHeight - panelHeight);
        }

        ImGui.SetNextWindowPos(new Vector2(panelX, panelY), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(panelWidth, panelHeight), ImGuiCond.Always);
        ImGui.SetNextWindowBgAlpha(0.95f);

        var popupFlags = ImGuiWindowFlags.NoDecoration
                         | ImGuiWindowFlags.NoMove
                         | ImGuiWindowFlags.NoSavedSettings
                         | ImGuiWindowFlags.NoNav
                         | ImGuiWindowFlags.NoInputs
                         | ImGuiWindowFlags.NoFocusOnAppearing
                         | ImGuiWindowFlags.NoBringToFrontOnFocus;

        if (!ImGui.Begin("##ConsoleSuggestionsPopup", popupFlags))
        {
            ImGui.End();
            return;
        }

        if (ImGui.BeginChild(
                "##ConsoleSuggestionsScroll",
                Vector2.Zero,
                ImGuiChildFlags.None,
                ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoInputs))
        {
            var drawList = ImGui.GetWindowDrawList();
            var selectedBgColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.18f, 0.24f, 0.32f, 0.95f));

            for (int i = 0; i < _consoleSuggestions.Count; i++)
            {
                var item = _consoleSuggestions[i];
                bool selected = i == _consoleSuggestionIndex;
                string kindLabel = item.Entry.Kind == ConsoleEntryKind.CVar ? "cvar" : "cmd";
                string rowText = $"{item.Entry.Usage}  [{kindLabel}]";
                if (selected)
                {
                    var min = ImGui.GetCursorScreenPos();
                    float rowHeight = ImGui.GetTextLineHeightWithSpacing();
                    var max = new Vector2(min.X + panelWidth, min.Y + rowHeight);
                    drawList.AddRectFilled(min, max, selectedBgColor);
                }

                ImGui.TextUnformatted(rowText);

                if (selected)
                    ImGui.SetScrollHereY();
            }
        }

        ImGui.EndChild();
        ImGui.End();
    }

    private static void RefreshSuggestions(string input)
    {
        var query = (input ?? string.Empty).Trim();
        if (query.Length == 0)
        {
            _consoleSuggestions.Clear();
            _consoleSuggestionIndex = -1;
            return;
        }

        string previousActiveName = _consoleSuggestionIndex >= 0 && _consoleSuggestionIndex < _consoleSuggestions.Count
            ? _consoleSuggestions[_consoleSuggestionIndex].Entry.Name
            : string.Empty;

        var scored = new List<ConsoleSuggestionItem>();
        var entries = ConsoleBackend.GetRegisteredEntries();
        foreach (var entry in entries)
        {
            if (TryScoreSuggestion(query, entry, out int score))
                scored.Add(new ConsoleSuggestionItem(entry, score));
        }

        scored.Sort(static (left, right) =>
        {
            int byScore = right.Score.CompareTo(left.Score);
            if (byScore != 0)
                return byScore;

            int byName = string.Compare(left.Entry.Name, right.Entry.Name, StringComparison.OrdinalIgnoreCase);
            if (byName != 0)
                return byName;

            return string.Compare(left.Entry.Usage, right.Entry.Usage, StringComparison.OrdinalIgnoreCase);
        });

        if (scored.Count > ConsoleSuggestionMaxMatches)
            scored.RemoveRange(ConsoleSuggestionMaxMatches, scored.Count - ConsoleSuggestionMaxMatches);

        _consoleSuggestions.Clear();
        _consoleSuggestions.AddRange(scored);

        if (_consoleSuggestions.Count == 0)
        {
            _consoleSuggestionIndex = -1;
            return;
        }

        if (!string.IsNullOrEmpty(previousActiveName))
        {
            int existingIndex = _consoleSuggestions.FindIndex(item =>
                string.Equals(item.Entry.Name, previousActiveName, StringComparison.OrdinalIgnoreCase));
            if (existingIndex >= 0)
            {
                _consoleSuggestionIndex = existingIndex;
                return;
            }
        }

        _consoleSuggestionIndex = 0;
    }

    private static bool TryScoreSuggestion(string query, ConsoleEntryDescriptor entry, out int score)
    {
        score = 0;
        if (string.IsNullOrWhiteSpace(query))
            return false;

        var normalizedQuery = query.Trim();
        var haystack = $"{entry.Name} {entry.Usage} {entry.Description}";
        if (!TrySubsequenceScore(normalizedQuery, haystack, out score))
            return false;

        int contiguousIndex = haystack.IndexOf(normalizedQuery, StringComparison.OrdinalIgnoreCase);
        if (contiguousIndex >= 0)
            score += 500 - Math.Min(400, contiguousIndex);

        if (entry.Name.StartsWith(normalizedQuery, StringComparison.OrdinalIgnoreCase))
            score += 1000;
        else if (entry.Usage.StartsWith(normalizedQuery, StringComparison.OrdinalIgnoreCase))
            score += 350;

        score += Math.Max(0, 80 - entry.Name.Length);
        return true;
    }

    private static bool TrySubsequenceScore(string query, string haystack, out int score)
    {
        score = 0;
        if (string.IsNullOrEmpty(query))
            return false;

        int queryIndex = 0;
        int streak = 0;
        int lastMatchIndex = -2;

        for (int i = 0; i < haystack.Length && queryIndex < query.Length; i++)
        {
            if (char.ToLowerInvariant(haystack[i]) != char.ToLowerInvariant(query[queryIndex]))
                continue;

            bool contiguous = i == lastMatchIndex + 1;
            streak = contiguous ? streak + 1 : 1;
            score += 10 + streak * 4;

            if (i == 0 || char.IsWhiteSpace(haystack[i - 1]))
                score += 6;

            lastMatchIndex = i;
            queryIndex++;
        }

        return queryIndex == query.Length;
    }

    private static string ReplaceFirstToken(string input, string replacement)
    {
        SplitFirstToken(input, out var leading, out _, out var suffix);
        return string.Concat(leading, replacement ?? string.Empty, suffix);
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

        // --- Rendering cvars ---
        ConsoleBackend.RegisterCVar(new ConsoleCVar(
            "r_postprocess",
            "r_postprocess [0|1]",
            "Enable or disable post-processing in runtime and Play/Simulate (1=on, 0=off).",
            RenderRuntimeCvars.GetPostProcessingValue,
            RenderRuntimeCvars.TrySetPostProcessing));

        ConsoleBackend.RegisterCVar(new ConsoleCVar(
            "r_showstats",
            "r_showstats [0-3]",
            "Stats overlay mode (0=off, 1=fps, 2=advanced, 3=verbose).",
            () => ((int)_statsMode).ToString(),
            value =>
            {
                if (!int.TryParse(value, out var v) || v < 0 || v > 3)
                    return false;
                _statsMode = (StatsOverlayMode)v;
                return true;
            }));

        ConsoleBackend.RegisterCVar(new ConsoleCVar(
            "r_profiler",
            "r_profiler [0|1]",
            "Enable or disable the frame profiler (1=on, 0=off).",
            () => FrameProfiler.Enabled ? "1" : "0",
            value =>
            {
                if (!TryParseBool01(value, out var enabled))
                    return false;
                FrameProfiler.Enabled = enabled;
                return true;
            }));

        ConsoleBackend.RegisterCVar(new ConsoleCVar(
            "r_ambient",
            "r_ambient <r> <g> <b>",
            "Override default ambient light (0-1 per channel). Use 'r_ambient default' to reset.",
            RenderRuntimeCvars.GetAmbientValue,
            RenderRuntimeCvars.TrySetAmbient));

        ConsoleBackend.RegisterCVar(new ConsoleCVar(
            "r_maxfps",
            "r_maxfps [0-500]",
            "Set target FPS (0 = uncapped).",
            RenderRuntimeCvars.GetTargetFpsValue,
            value =>
            {
                if (!RenderRuntimeCvars.TrySetTargetFps(value))
                    return false;
                Raylib.SetTargetFPS(RenderRuntimeCvars.TargetFps);
                return true;
            }));

        ConsoleBackend.RegisterCVar(new ConsoleCVar(
            "r_vsync",
            "r_vsync [0|1]",
            "Toggle VSync (1=on, 0=off).",
            () => Raylib.IsWindowState(ConfigFlags.VSyncHint) ? "1" : "0",
            value =>
            {
                if (!TryParseBool01(value, out var enabled))
                    return false;
                if (enabled)
                    Raylib.SetWindowState(ConfigFlags.VSyncHint);
                else
                    Raylib.ClearWindowState(ConfigFlags.VSyncHint);
                return true;
            }));

        // --- Audio cvars ---
        RegisterVolumeCVar("snd_master", "Master audio bus volume.",
            () => AudioProjectSettings.Current.MasterVolume,
            v => { AudioProjectSettings.Current.MasterVolume = v; AudioApi.SetBusVolume(AudioBusId.Master, v); });
        RegisterVolumeCVar("snd_music", "Music bus volume.",
            () => AudioProjectSettings.Current.MusicVolume,
            v => { AudioProjectSettings.Current.MusicVolume = v; AudioApi.SetBusVolume(AudioBusId.Music, v); });
        RegisterVolumeCVar("snd_sfx", "SFX bus volume.",
            () => AudioProjectSettings.Current.SfxVolume,
            v => { AudioProjectSettings.Current.SfxVolume = v; AudioApi.SetBusVolume(AudioBusId.Sfx, v); });
        RegisterVolumeCVar("snd_ui", "UI bus volume.",
            () => AudioProjectSettings.Current.UiVolume,
            v => { AudioProjectSettings.Current.UiVolume = v; AudioApi.SetBusVolume(AudioBusId.Ui, v); });
        RegisterVolumeCVar("snd_voice", "Voice bus volume.",
            () => AudioProjectSettings.Current.VoiceVolume,
            v => { AudioProjectSettings.Current.VoiceVolume = v; AudioApi.SetBusVolume(AudioBusId.Voice, v); });
        RegisterVolumeCVar("snd_ambient", "Ambient bus volume.",
            () => AudioProjectSettings.Current.AmbientVolume,
            v => { AudioProjectSettings.Current.AmbientVolume = v; AudioApi.SetBusVolume(AudioBusId.Ambient, v); });

        // --- Physics cvars ---
        ConsoleBackend.RegisterCVar(new ConsoleCVar(
            "physics_gravity",
            "physics_gravity <x> <y> <z>",
            "Scene gravity vector.",
            () =>
            {
                var scene = SceneManager.Instance.ActiveScene;
                if (scene == null) return "no scene";
                var g = scene.PhysicsSettings.Gravity;
                return $"{g.X:F2} {g.Y:F2} {g.Z:F2}";
            },
            value =>
            {
                var scene = SceneManager.Instance.ActiveScene;
                if (scene == null) return false;
                var parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 3) return false;
                if (!float.TryParse(parts[0], out var x) ||
                    !float.TryParse(parts[1], out var y) ||
                    !float.TryParse(parts[2], out var z))
                    return false;
                if (!float.IsFinite(x) || !float.IsFinite(y) || !float.IsFinite(z))
                    return false;
                scene.PhysicsSettings.Gravity = new Vector3(x, y, z);
                return true;
            }));

        ConsoleBackend.RegisterCVar(new ConsoleCVar(
            "physics_timestep",
            "physics_timestep [value]",
            "Fixed physics simulation timestep in seconds.",
            () => PhysicsProjectSettings.Current.FixedTimestep.ToString("F4"),
            value =>
            {
                if (!TryParseFloatClamped(value, 1f / 240f, 1f / 15f, out var v))
                    return false;
                PhysicsProjectSettings.Current.FixedTimestep = v;
                return true;
            }));

        ConsoleBackend.RegisterCVar(new ConsoleCVar(
            "physics_interpolation",
            "physics_interpolation [0|1]",
            "Enable or disable physics interpolation (1=on, 0=off).",
            () => PhysicsProjectSettings.Current.InterpolationEnabled ? "1" : "0",
            value =>
            {
                if (!TryParseBool01(value, out var enabled))
                    return false;
                PhysicsProjectSettings.Current.InterpolationEnabled = enabled;
                return true;
            }));

        // --- Commands ---
        ConsoleBackend.RegisterCommand("clear", "clear", "Clear console history.",
            _ =>
            {
                ConsoleHistory.Clear();
                return new ConsoleExecutionResult(true, new[] { "Console cleared." });
            });

        ConsoleBackend.RegisterCommand("scene", "scene", "Display active scene info.",
            _ =>
            {
                var scene = SceneManager.Instance.ActiveScene;
                if (scene == null)
                    return new ConsoleExecutionResult(true, new[] { "No active scene." });

                var lines = new List<string>
                {
                    $"Scene: {scene.Name}",
                    $"Entities: {scene.Entities.Count}",
                    $"Cameras: {scene.Cameras.Count}",
                    $"Lights: {scene.Lights.Count}"
                };
                return new ConsoleExecutionResult(true, lines);
            });

        ConsoleBackend.RegisterCommand("echo", "echo <text>", "Print text to console.",
            args =>
            {
                var text = args.Count > 0 ? string.Join(' ', args) : string.Empty;
                return new ConsoleExecutionResult(true, new[] { text });
            });

        ConsoleBackend.RegisterCommand("quit", "quit", "Exit the application.",
            _ =>
            {
                Raylib.CloseWindow();
                return new ConsoleExecutionResult(true, new[] { "Quitting..." });
            });

        ConsoleBackend.RegisterCommand("open_scene", "open_scene <path>", "Load a .fscene file by path.",
            args =>
            {
                if (args.Count == 0)
                    return new ConsoleExecutionResult(false, new[] { "Usage: open_scene <path>" });

                var path = string.Join(' ', args);
                if (!File.Exists(path))
                    return new ConsoleExecutionResult(false, new[] { $"File not found: {path}" });

                var scene = SceneManager.Instance.LoadScene(path);
                return scene != null
                    ? new ConsoleExecutionResult(true, new[] { $"Loaded scene: {path}" })
                    : new ConsoleExecutionResult(false, new[] { $"Failed to load scene: {path}" });
            });

        ConsoleBackend.RegisterCommand("restart_scene", "restart_scene", "Reload the current scene from disk.",
            _ =>
            {
                var active = SceneManager.Instance.ActiveScene;
                if (active == null)
                    return new ConsoleExecutionResult(false, new[] { "No active scene." });

                var filePath = active.FilePath;
                if (string.IsNullOrEmpty(filePath))
                    return new ConsoleExecutionResult(false, new[] { "Active scene has no file path (unsaved scene)." });

                if (!File.Exists(filePath))
                    return new ConsoleExecutionResult(false, new[] { $"Scene file not found: {filePath}" });

                var scene = SceneManager.Instance.LoadScene(filePath);
                return scene != null
                    ? new ConsoleExecutionResult(true, new[] { $"Reloaded scene: {filePath}" })
                    : new ConsoleExecutionResult(false, new[] { $"Failed to reload scene: {filePath}" });
            });

        ConsoleBackend.RegisterCommand("entities", "entities", "List all entities in the active scene.",
            _ =>
            {
                var scene = SceneManager.Instance.ActiveScene;
                if (scene == null)
                    return new ConsoleExecutionResult(true, new[] { "No active scene." });

                var entities = scene.Entities;
                if (entities.Count == 0)
                    return new ConsoleExecutionResult(true, new[] { "Scene has no entities." });

                const int maxDisplay = 50;
                var lines = new List<string>();
                int count = Math.Min(entities.Count, maxDisplay);
                for (int i = 0; i < count; i++)
                {
                    var e = entities[i];
                    var status = e.Active ? "active" : "inactive";
                    lines.Add($"  {e.Name} [{status}]");
                }

                if (entities.Count > maxDisplay)
                    lines.Add($"  ... and {entities.Count - maxDisplay} more");

                lines.Insert(0, $"Entities ({entities.Count}):");
                return new ConsoleExecutionResult(true, lines);
            });

        _consoleBackendInitialized = true;
    }

    private static void RegisterVolumeCVar(string name, string description, Func<float> getter, Action<float> setter)
    {
        ConsoleBackend.RegisterCVar(new ConsoleCVar(
            name,
            $"{name} [0.0-2.0]",
            description,
            () => getter().ToString("F2"),
            value =>
            {
                if (!TryParseFloatClamped(value, 0f, 2f, out var v))
                    return false;
                setter(v);
                return true;
            }));
    }

    private static bool TryParseBool01(string value, out bool enabled)
    {
        enabled = false;
        if (value == null) return false;
        switch (value.Trim())
        {
            case "1": enabled = true; return true;
            case "0": enabled = false; return true;
            default: return false;
        }
    }

    private static bool TryParseFloatClamped(string value, float min, float max, out float result)
    {
        result = 0f;
        if (!float.TryParse(value, out var parsed) || !float.IsFinite(parsed))
            return false;
        if (parsed < min || parsed > max)
            return false;
        result = parsed;
        return true;
    }
}
