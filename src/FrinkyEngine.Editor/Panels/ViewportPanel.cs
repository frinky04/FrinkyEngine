using System.Numerics;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;

namespace FrinkyEngine.Editor.Panels;

public class ViewportPanel
{
    private readonly EditorApplication _app;
    private RenderTexture2D _renderTexture;
    private int _lastWidth;
    private int _lastHeight;
    private bool _isHovered;

    public ViewportPanel(EditorApplication app)
    {
        _app = app;
    }

    public void EnsureRenderTexture(int width, int height)
    {
        if (width <= 0 || height <= 0) return;
        if (width == _lastWidth && height == _lastHeight) return;

        if (_lastWidth > 0)
            Raylib.UnloadRenderTexture(_renderTexture);

        _renderTexture = Raylib.LoadRenderTexture(width, height);
        _lastWidth = width;
        _lastHeight = height;
    }

    public void Draw()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        if (ImGui.Begin("Viewport"))
        {
            var size = ImGui.GetContentRegionAvail();
            int w = (int)size.X;
            int h = (int)size.Y;

            if (w > 0 && h > 0)
            {
                EnsureRenderTexture(w, h);

                var camera = _app.Mode == EditorMode.Play && _app.CurrentScene?.MainCamera != null
                    ? _app.CurrentScene.MainCamera.BuildCamera3D()
                    : _app.EditorCamera.Camera3D;

                if (_app.CurrentScene != null)
                    _app.SceneRenderer.Render(_app.CurrentScene, camera, _renderTexture);

                rlImGui.ImageRenderTexture(_renderTexture);
            }

            _isHovered = ImGui.IsWindowHovered();
        }
        ImGui.End();
        ImGui.PopStyleVar();

        _app.EditorCamera.Update(Raylib.GetFrameTime(), _isHovered && _app.Mode == EditorMode.Edit);
    }
}
