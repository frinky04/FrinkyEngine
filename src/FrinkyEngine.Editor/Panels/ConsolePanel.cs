using System.Numerics;
using System.Text;
using FrinkyEngine.Core.Rendering;
using Hexa.NET.ImGui;

namespace FrinkyEngine.Editor.Panels;

public class ConsolePanel
{
    private readonly EditorApplication _app;
    private bool _autoScroll = true;
    private bool _showInfo = true;
    private bool _showWarning = true;
    private bool _showError = true;
    private bool _showRaylib;
    private bool _showTimestamps = true;
    private string _searchFilter = string.Empty;

    public ConsolePanel(EditorApplication app)
    {
        _app = app;
    }

    public void Draw()
    {
        if (ImGui.Begin("Console"))
        {
            DrawToolbar();
            ImGui.Separator();
            DrawLogRegion();
        }
        ImGui.End();
    }

    private void DrawToolbar()
    {
        if (ImGui.Button("Clear"))
            FrinkyLog.Clear();

        ImGui.SameLine();
        if (ImGui.Button("Copy All"))
        {
            var sb = new StringBuilder();
            foreach (var e in FrinkyLog.Entries)
            {
                if (PassesFilter(e))
                    sb.AppendLine(FormatEntry(e));
            }
            ImGui.SetClipboardText(sb.ToString());
        }

        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.8f, 0.8f, 0.8f, 1f));
        ImGui.Checkbox("Info", ref _showInfo);
        ImGui.PopStyleColor();

        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1f, 0.85f, 0f, 1f));
        ImGui.Checkbox("Warning", ref _showWarning);
        ImGui.PopStyleColor();

        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1f, 0.3f, 0.3f, 1f));
        ImGui.Checkbox("Error", ref _showError);
        ImGui.PopStyleColor();

        ImGui.SameLine();
        ImGui.Text("|");

        ImGui.SameLine();
        ImGui.Checkbox("Raylib", ref _showRaylib);

        ImGui.SameLine();
        ImGui.Checkbox("Timestamps", ref _showTimestamps);

        ImGui.SameLine();
        ImGui.Checkbox("Auto-scroll", ref _autoScroll);

        // Entry counts
        int infoCount = 0, warnCount = 0, errCount = 0;
        foreach (var e in FrinkyLog.Entries)
        {
            switch (e.Level)
            {
                case LogLevel.Info: infoCount++; break;
                case LogLevel.Warning: warnCount++; break;
                case LogLevel.Error: errCount++; break;
            }
        }

        ImGui.SameLine();
        ImGui.TextDisabled($"({infoCount}i / {warnCount}w / {errCount}e)");

        // Search filter
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##LogSearch", "Search log...", ref _searchFilter, 256);
    }

    private void DrawLogRegion()
    {
        ImGui.BeginChild("LogRegion", Vector2.Zero, ImGuiChildFlags.None, ImGuiWindowFlags.HorizontalScrollbar);

        for (int i = 0; i < FrinkyLog.Entries.Count; i++)
        {
            var entry = FrinkyLog.Entries[i];

            if (!PassesFilter(entry))
                continue;

            var text = FormatEntry(entry);

            var color = entry.Level switch
            {
                LogLevel.Warning => new Vector4(1f, 0.85f, 0f, 1f),
                LogLevel.Error => new Vector4(1f, 0.3f, 0.3f, 1f),
                _ => new Vector4(0.8f, 0.8f, 0.8f, 1f)
            };

            ImGui.PushID(i);
            ImGui.PushStyleColor(ImGuiCol.Text, color);
            ImGui.Selectable(text, false, ImGuiSelectableFlags.None);
            ImGui.PopStyleColor();

            if (ImGui.BeginPopupContextItem("LogEntryCtx"))
            {
                if (ImGui.MenuItem("Copy"))
                    ImGui.SetClipboardText(text);
                ImGui.EndPopup();
            }
            ImGui.PopID();
        }

        if (_autoScroll && ImGui.GetScrollY() >= ImGui.GetScrollMaxY())
            ImGui.SetScrollHereY(1.0f);

        ImGui.EndChild();
    }

    private string FormatEntry(LogEntry entry)
    {
        return _showTimestamps
            ? $"[{entry.Timestamp:HH:mm:ss}] [{entry.Source}/{entry.Level}] {entry.Message}"
            : $"[{entry.Source}/{entry.Level}] {entry.Message}";
    }

    private bool PassesFilter(LogEntry entry)
    {
        if (!_showRaylib && entry.Source == "Raylib")
            return false;

        var levelPass = entry.Level switch
        {
            LogLevel.Info => _showInfo,
            LogLevel.Warning => _showWarning,
            LogLevel.Error => _showError,
            _ => true
        };

        if (!levelPass)
            return false;

        if (_searchFilter.Length > 0 &&
            !entry.Message.Contains(_searchFilter, StringComparison.OrdinalIgnoreCase) &&
            !entry.Source.Contains(_searchFilter, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }
}
