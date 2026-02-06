using System.Numerics;
using FrinkyEngine.Core.Rendering;
using ImGuiNET;

namespace FrinkyEngine.Editor.Panels;

public class ConsolePanel
{
    private readonly EditorApplication _app;
    private bool _autoScroll = true;

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
            ImGui.Checkbox("Auto-scroll", ref _autoScroll);

            ImGui.Separator();

            ImGui.BeginChild("LogRegion", Vector2.Zero, ImGuiChildFlags.None, ImGuiWindowFlags.HorizontalScrollbar);

            foreach (var entry in FrinkyLog.Entries)
            {
                var color = entry.Level switch
                {
                    LogLevel.Warning => new Vector4(1f, 0.85f, 0f, 1f),
                    LogLevel.Error => new Vector4(1f, 0.3f, 0.3f, 1f),
                    _ => new Vector4(0.8f, 0.8f, 0.8f, 1f)
                };

                ImGui.PushStyleColor(ImGuiCol.Text, color);
                ImGui.TextUnformatted($"[{entry.Timestamp:HH:mm:ss}] [{entry.Level}] {entry.Message}");
                ImGui.PopStyleColor();
            }

            if (_autoScroll && ImGui.GetScrollY() >= ImGui.GetScrollMaxY())
                ImGui.SetScrollHereY(1.0f);

            ImGui.EndChild();
        }
        ImGui.End();
    }
}
