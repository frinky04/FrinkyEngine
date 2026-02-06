using System.Numerics;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;

namespace FrinkyEngine.Editor.Panels;

public class ViewportPanel
{
    private static readonly Color SelectionOutlineColor = new(255, 170, 0, 255);
    private const float SelectionOutlineWidthPixels = 1.5f;

    private readonly EditorApplication _app;
    private RenderTexture2D _renderTexture;
    private RenderTexture2D _selectionMaskTexture;
    private RenderTexture2D _outlineCompositeTexture;
    private Shader _selectionOutlinePostShader;
    private bool _selectionOutlinePostShaderLoaded;
    private int _maskTextureLoc = -1;
    private int _texelSizeLoc = -1;
    private int _outlineColorLoc = -1;
    private int _outlineWidthLoc = -1;
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
        {
            Raylib.UnloadRenderTexture(_renderTexture);
            Raylib.UnloadRenderTexture(_selectionMaskTexture);
            Raylib.UnloadRenderTexture(_outlineCompositeTexture);
        }

        _renderTexture = Raylib.LoadRenderTexture(width, height);
        _selectionMaskTexture = Raylib.LoadRenderTexture(width, height);
        _outlineCompositeTexture = Raylib.LoadRenderTexture(width, height);
        Raylib.SetTextureFilter(_selectionMaskTexture.Texture, TextureFilter.Point);
        _lastWidth = width;
        _lastHeight = height;

        EnsureSelectionOutlineShaderLoaded();
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
                    var textureToDisplay = _renderTexture;

                    _app.SceneRenderer.Render(_app.CurrentScene, camera, _renderTexture,
                        () =>
                        {
                            if (isEditorMode)
                            {
                                gizmo.Draw(camera, selectedEntities, selected);
                                EditorGizmos.DrawAll(_app.CurrentScene, camera);
                                foreach (var selectedEntity in selectedEntities)
                                    EditorGizmos.DrawSelectionFallbackHighlight(selectedEntity);
                            }
                        },
                        isEditorMode: isEditorMode);

                    if (isEditorMode && selectedEntities.Count > 0)
                    {
                        _app.SceneRenderer.RenderSelectionMask(
                            _app.CurrentScene,
                            camera,
                            selectedEntities,
                            _selectionMaskTexture,
                            isEditorMode: true);

                        if (_selectionOutlinePostShaderLoaded)
                        {
                            CompositeSelectionOutline(w, h);
                            textureToDisplay = _outlineCompositeTexture;
                        }
                    }

                    var imageScreenPos = ImGui.GetCursorScreenPos();
                    rlImGui.ImageRenderTexture(textureToDisplay);
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
                }
                else
                {
                    var imageScreenPos = ImGui.GetCursorScreenPos();
                    rlImGui.ImageRenderTexture(_renderTexture);
                    bool toolbarHovered = DrawTransformModeToggle(gizmo);

                    _isHovered = ImGui.IsWindowHovered();
                    if (_isHovered && !toolbarHovered && _app.Mode == EditorMode.Edit)
                    {
                        var mousePos = ImGui.GetMousePos();
                        var localMouse = mousePos - imageScreenPos;
                        gizmo.Update(camera, selectedEntities, selected, localMouse, new Vector2(w, h));
                    }
                    else if (!_isHovered)
                    {
                        gizmo.Update(camera, Array.Empty<Core.ECS.Entity>(), null, Vector2.Zero, Vector2.One);
                    }
                }

                // Gizmo drag batching for undo
                if (gizmo.IsDragging && !_wasGizmoDragging)
                {
                    _app.UndoRedo.BeginBatch(_app.GetSelectedEntityIds());
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

    private void EnsureSelectionOutlineShaderLoaded()
    {
        if (_selectionOutlinePostShaderLoaded)
            return;

        const string vsPath = "Shaders/selection_outline_post.vs";
        const string fsPath = "Shaders/selection_outline_post.fs";
        if (!File.Exists(vsPath) || !File.Exists(fsPath))
            return;

        _selectionOutlinePostShader = Raylib.LoadShader(vsPath, fsPath);
        _maskTextureLoc = Raylib.GetShaderLocation(_selectionOutlinePostShader, "maskTexture");
        _texelSizeLoc = Raylib.GetShaderLocation(_selectionOutlinePostShader, "texelSize");
        _outlineColorLoc = Raylib.GetShaderLocation(_selectionOutlinePostShader, "outlineColor");
        _outlineWidthLoc = Raylib.GetShaderLocation(_selectionOutlinePostShader, "outlineWidth");
        _selectionOutlinePostShaderLoaded = true;
    }

    private void CompositeSelectionOutline(int width, int height)
    {
        if (!_selectionOutlinePostShaderLoaded)
            return;

        Raylib.BeginTextureMode(_outlineCompositeTexture);
        Raylib.ClearBackground(new Color(0, 0, 0, 0));
        Raylib.BeginShaderMode(_selectionOutlinePostShader);

        if (_maskTextureLoc >= 0)
            Raylib.SetShaderValueTexture(_selectionOutlinePostShader, _maskTextureLoc, _selectionMaskTexture.Texture);

        if (_texelSizeLoc >= 0)
        {
            float[] texelSize = { 1.0f / width, 1.0f / height };
            Raylib.SetShaderValue(_selectionOutlinePostShader, _texelSizeLoc, texelSize, ShaderUniformDataType.Vec2);
        }

        if (_outlineColorLoc >= 0)
        {
            float[] outlineColor =
            {
                SelectionOutlineColor.R / 255f,
                SelectionOutlineColor.G / 255f,
                SelectionOutlineColor.B / 255f,
                SelectionOutlineColor.A / 255f
            };
            Raylib.SetShaderValue(_selectionOutlinePostShader, _outlineColorLoc, outlineColor, ShaderUniformDataType.Vec4);
        }

        if (_outlineWidthLoc >= 0)
        {
            float[] outlineWidth = { SelectionOutlineWidthPixels };
            Raylib.SetShaderValue(_selectionOutlinePostShader, _outlineWidthLoc, outlineWidth, ShaderUniformDataType.Float);
        }

        DrawFullscreenTexture(_renderTexture.Texture, width, height);
        Raylib.EndShaderMode();
        Raylib.EndTextureMode();
    }

    private static void DrawFullscreenTexture(Texture2D source, int width, int height)
    {
        var src = new Rectangle(0, 0, width, -height);
        var dst = new Rectangle(0, 0, width, height);
        Raylib.DrawTexturePro(source, src, dst, Vector2.Zero, 0.0f, Color.White);
    }

    public void Shutdown()
    {
        if (_lastWidth > 0)
        {
            Raylib.UnloadRenderTexture(_renderTexture);
            Raylib.UnloadRenderTexture(_selectionMaskTexture);
            Raylib.UnloadRenderTexture(_outlineCompositeTexture);
            _lastWidth = 0;
            _lastHeight = 0;
        }

        if (_selectionOutlinePostShaderLoaded)
        {
            Raylib.UnloadShader(_selectionOutlinePostShader);
            _selectionOutlinePostShaderLoaded = false;
        }
    }
}
