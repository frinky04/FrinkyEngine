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
    private bool _showRaylib = true;

    public ConsolePanel(EditorApplication app)
    {
        _app = app;
    }

    public void Draw()
    {
        if (ImGui.Begin("Console"))
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
                        sb.AppendLine($"[{e.Timestamp:HH:mm:ss}] [{e.Source}/{e.Level}] {e.Message}");
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
            ImGui.Checkbox("Auto-scroll", ref _autoScroll);

            ImGui.Separator();

            ImGui.BeginChild("LogRegion", Vector2.Zero, ImGuiChildFlags.None, ImGuiWindowFlags.HorizontalScrollbar);

            for (int i = 0; i < FrinkyLog.Entries.Count; i++)
            {
                var entry = FrinkyLog.Entries[i];

                if (!PassesFilter(entry))
                    continue;

                var text = $"[{entry.Timestamp:HH:mm:ss}] [{entry.Source}/{entry.Level}] {entry.Message}";

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
        ImGui.End();
    }

    private bool PassesFilter(LogEntry entry)
    {
        if (!_showRaylib && entry.Source == "Raylib")
            return false;

        return entry.Level switch
        {
            LogLevel.Info => _showInfo,
            LogLevel.Warning => _showWarning,
            LogLevel.Error => _showError,
            _ => true
        };
    }
}
