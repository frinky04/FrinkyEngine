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
    private bool _wasGizmoDragging;

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

                var gizmo = _app.GizmoSystem;
                var selected = _app.SelectedEntity;
                var selectedEntities = _app.SelectedEntities;

                if (_app.CurrentScene != null)
                {
                    bool isEditorMode = _app.Mode == EditorMode.Edit;
                    _app.SceneRenderer.Render(_app.CurrentScene, camera, _renderTexture,
                        () =>
                        {
                            if (isEditorMode)
                            {
                                gizmo.Draw(camera, selectedEntities, selected);
                                EditorGizmos.DrawAll(_app.CurrentScene, camera);
                                foreach (var selectedEntity in selectedEntities)
                                    EditorGizmos.DrawSelectionHighlight(selectedEntity);
                            }
                        },
                        isEditorMode: isEditorMode);
                }

                var imageScreenPos = ImGui.GetCursorScreenPos();
                rlImGui.ImageRenderTexture(_renderTexture);
                bool toolbarHovered = DrawTransformModeToggle(gizmo);

                // Gizmo input: compute viewport-local mouse position
                _isHovered = ImGui.IsWindowHovered();
                if (_isHovered && !toolbarHovered && _app.Mode == EditorMode.Edit)
                {
                    var mousePos = ImGui.GetMousePos();
                    var localMouse = mousePos - imageScreenPos;
                    gizmo.Update(camera, selectedEntities, selected, localMouse, new Vector2(w, h));

                    // Viewport picking: left-click selects entity, but gizmo and camera fly take priority
                    if (Raylib.IsMouseButtonPressed(MouseButton.Left)
                        && !gizmo.IsDragging
                        && gizmo.HoveredAxis < 0
                        && !Raylib.IsMouseButtonDown(MouseButton.Right)
                        && _app.CurrentScene != null)
                    {
                        var pickedEntity = _app.PickingSystem.Pick(
                            _app.CurrentScene, camera, localMouse, new Vector2(w, h));

                        if (ImGui.GetIO().KeyCtrl)
                        {
                            if (pickedEntity != null)
                                _app.ToggleSelection(pickedEntity);
                        }
                        else
                        {
                            _app.SetSingleSelection(pickedEntity);
                        }
                    }
                }
                else if (!_isHovered)
                {
                    // Clear hover state when viewport not hovered
                    gizmo.Update(camera, Array.Empty<Core.ECS.Entity>(), null, Vector2.Zero, Vector2.One);
                }

                // Gizmo drag batching for undo
                if (gizmo.IsDragging && !_wasGizmoDragging)
                {
                    _app.UndoRedo.BeginBatch();
                }
                else if (!gizmo.IsDragging && _wasGizmoDragging)
                {
                    _app.UndoRedo.EndBatch(_app.CurrentScene, _app.GetSelectedEntityIds());
                }
                _wasGizmoDragging = gizmo.IsDragging;
            }
            else
            {
                _isHovered = ImGui.IsWindowHovered();
            }
        }
        else
        {
            _isHovered = false;
        }
        ImGui.End();
        ImGui.PopStyleVar();

        _app.EditorCamera.Update(Raylib.GetFrameTime(), _isHovered && _app.Mode == EditorMode.Edit);
    }

    private static bool DrawTransformModeToggle(GizmoSystem gizmo)
    {
        ImGui.SetCursorPos(new Vector2(10, 10));
        var label = gizmo.MultiMode == MultiTransformMode.Independent
            ? "Transform: Independent"
            : "Transform: Relative";

        if (ImGui.Button(label))
        {
            gizmo.MultiMode = gizmo.MultiMode == MultiTransformMode.Independent
                ? MultiTransformMode.Relative
                : MultiTransformMode.Independent;
        }

        return ImGui.IsItemHovered();
    }
}
