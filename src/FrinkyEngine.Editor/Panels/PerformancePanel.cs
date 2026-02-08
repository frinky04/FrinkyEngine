using System.Numerics;
using Hexa.NET.ImGui;
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
        ImGui.SetNextWindowSize(new Vector2(420, 310), ImGuiCond.FirstUseEver);

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
                0, (string?)null, 0f, graphMax, new Vector2(ImGui.GetContentRegionAvail().X, 50));

            int entityCount = _app.CurrentScene?.Entities.Count ?? 0;
            ImGui.Text($"Entities: {entityCount}");

            // Condensed lighting stats
            var lightStats = _app.SceneRenderer.GetForwardPlusFrameStats();
            if (lightStats.Valid)
            {
                ImGui.Separator();
                ImGui.Text($"Lighting (Forward+)  Scene: {lightStats.SceneLights}  Visible: {lightStats.VisibleLights}  Dir: {lightStats.DirectionalLights}  Pt: {lightStats.PointLights}  Sky: {lightStats.Skylights}");
                var warningParts = new List<string>();
                if (lightStats.ClippedLights > 0)
                    warningParts.Add($"Clipped: {lightStats.ClippedLights}");
                if (lightStats.DroppedTileLinks > 0)
                    warningParts.Add($"Dropped: {lightStats.DroppedTileLinks}");
                var warningText = warningParts.Count > 0 ? $"  {string.Join("  ", warningParts)}" : "";
                ImGui.Text($"Assigned: {lightStats.AssignedLights}/{lightStats.MaxLights}  Peak/tile: {lightStats.PeakLightsPerTile}/{lightStats.MaxLightsPerTile}{warningText}");
            }

            // Physics stats
            var physStats = _app.CurrentScene?.GetPhysicsFrameStats() ?? default;
            if (physStats.Valid)
            {
                ImGui.Separator();
                ImGui.Text($"Physics  Dyn: {physStats.DynamicBodies}  Kin: {physStats.KinematicBodies}  Static: {physStats.StaticBodies}  CC: {physStats.ActiveCharacterControllers}");
                ImGui.Text($"Substeps: {physStats.SubstepsThisFrame}  Step: {physStats.StepTimeMs:F2} ms");
            }

            var audioStats = _app.CurrentScene?.GetAudioFrameStats() ?? default;
            if (audioStats.Valid)
            {
                ImGui.Separator();
                ImGui.Text($"Audio  Voices: {audioStats.ActiveVoices}  Streaming: {audioStats.StreamingVoices}");
                ImGui.Text($"Stolen: {audioStats.StolenVoicesThisFrame}  Virtual: {audioStats.VirtualizedVoices}  Update: {audioStats.UpdateTimeMs:F2} ms");
            }
        }
        ImGui.End();

        if (!open) IsVisible = false;
    }
}
