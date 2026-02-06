using System.Numerics;
using ImGuiNET;
using Raylib_cs;

namespace FrinkyEngine.Editor.Panels;

public class PerformancePanel
{
    private const int BufferSize = 120;

    private readonly EditorApplication _app;
    private readonly float[] _frameTimes = new float[BufferSize];
    private int _frameIndex;

    public bool IsVisible { get; set; }

    public PerformancePanel(EditorApplication app)
    {
        _app = app;
    }

    public void Draw()
    {
        if (!IsVisible) return;

        var viewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowPos(
            new Vector2(viewport.WorkPos.X + 10, viewport.WorkPos.Y + 10),
            ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSize(new Vector2(420, 270), ImGuiCond.FirstUseEver);

        var flags = ImGuiWindowFlags.NoCollapse
                  | ImGuiWindowFlags.NoFocusOnAppearing
                  | ImGuiWindowFlags.NoNav;

        bool open = IsVisible;
        if (ImGui.Begin("Performance", ref open, flags))
        {
            // Update circular buffer
            float frameTime = Raylib.GetFrameTime() * 1000f;
            _frameTimes[_frameIndex] = frameTime;
            _frameIndex = (_frameIndex + 1) % BufferSize;

            // Compute min/max over buffer
            float min = float.MaxValue;
            float max = float.MinValue;
            for (int i = 0; i < BufferSize; i++)
            {
                if (_frameTimes[i] > 0)
                {
                    if (_frameTimes[i] < min) min = _frameTimes[i];
                    if (_frameTimes[i] > max) max = _frameTimes[i];
                }
            }
            if (min == float.MaxValue) min = 0;
            if (max == float.MinValue) max = 0;

            int fps = Raylib.GetFPS();
            ImGui.Text($"FPS: {fps}    Frame: {frameTime:F1} ms");
            ImGui.Text($"Min: {min:F1} ms    Max: {max:F1} ms");

            // Build ordered array for PlotLines (oldest first)
            var ordered = new float[BufferSize];
            for (int i = 0; i < BufferSize; i++)
                ordered[i] = _frameTimes[(_frameIndex + i) % BufferSize];

            float graphMax = max * 1.2f;
            if (graphMax < 1f) graphMax = 33.3f;

            ImGui.PlotLines("##frameTimes", ref ordered[0], BufferSize,
                0, null, 0f, graphMax, new Vector2(ImGui.GetContentRegionAvail().X, 50));

            int entityCount = _app.CurrentScene?.Entities.Count ?? 0;
            ImGui.Text($"Entities: {entityCount}");

            var lightStats = _app.SceneRenderer.GetForwardPlusFrameStats();
            if (lightStats.Valid)
            {
                ImGui.Separator();
                ImGui.Text("Lighting (Forward+)");
                ImGui.Text($"Scene: {lightStats.SceneLights}  Visible: {lightStats.VisibleLights}");
                ImGui.Text($"Directional: {lightStats.DirectionalLights}  Point: {lightStats.PointLights}  Skylight: {lightStats.Skylights}");
                ImGui.Text($"Assigned: {lightStats.AssignedLights}/{lightStats.MaxLights}  Clipped: {lightStats.ClippedLights}");
                ImGui.Text($"Tiles: {lightStats.TilesX}x{lightStats.TilesY}  TileSize: {lightStats.TileSize}");
                ImGui.Text($"Avg/Peak per tile: {lightStats.AverageLightsPerTile:F1}/{lightStats.PeakLightsPerTile}  Budget: {lightStats.MaxLightsPerTile}");
                ImGui.Text($"Dropped tile links: {lightStats.DroppedTileLinks}");
            }
        }
        ImGui.End();

        if (!open) IsVisible = false;
    }
}
