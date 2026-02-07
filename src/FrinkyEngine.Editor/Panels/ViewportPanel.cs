using System.Numerics;
using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.Components;
using FrinkyEngine.Core.Rendering;
using FrinkyEngine.Core.Serialization;
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
    private int _texelSizeLoc = -1;
    private int _outlineColorLoc = -1;
    private int _outlineWidthLoc = -1;
    private int _lastWidth;
    private int _lastHeight;
    private bool _isHovered;
    private bool _wasGizmoDragging;
    private System.Numerics.Vector3? _dragPreviewPosition;

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
        _dragPreviewPosition = null;

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
                    bool isEditorMode = _app.CanUseEditorViewportTools && !_app.IsGameViewEnabled;
                    var physicsHitboxDrawMode = ResolvePhysicsHitboxDrawMode();
                    var textureToDisplay = _renderTexture;

                    _app.SceneRenderer.Render(_app.CurrentScene, camera, _renderTexture,
                        () =>
                        {
                            if (physicsHitboxDrawMode != PhysicsHitboxDrawMode.Off)
                            {
                                EditorGizmos.DrawPhysicsHitboxes(_app.CurrentScene, selectedEntities, physicsHitboxDrawMode);
                            }

                            if (isEditorMode)
                            {
                                gizmo.Draw(camera, selectedEntities, selected);
                                EditorGizmos.DrawAll(_app.CurrentScene, camera);
                                foreach (var selectedEntity in selectedEntities)
                                    EditorGizmos.DrawSelectionFallbackHighlight(selectedEntity);
                            }

                            if (_dragPreviewPosition.HasValue)
                                DrawDropPreview(_dragPreviewPosition.Value);
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
                    if (isEditorMode)
                        HandleAssetDropTarget(camera, imageScreenPos, w, h);
                    bool toolbarHovered = false;
                    if (isEditorMode)
                        toolbarHovered = DrawViewportToolbar(gizmo);

                    // Gizmo input: compute viewport-local mouse position
                    _isHovered = ImGui.IsWindowHovered();
                    if (_isHovered && !toolbarHovered && isEditorMode)
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
                    else
                    {
                        gizmo.Update(camera, Array.Empty<Core.ECS.Entity>(), null, Vector2.Zero, Vector2.One);
                    }
                }
                else
                {
                    bool isEditorMode = _app.CanUseEditorViewportTools && !_app.IsGameViewEnabled;
                    var imageScreenPos = ImGui.GetCursorScreenPos();
                    rlImGui.ImageRenderTexture(_renderTexture);
                    if (isEditorMode)
                        HandleAssetDropTarget(camera, imageScreenPos, w, h);
                    bool toolbarHovered = false;
                    if (isEditorMode)
                        toolbarHovered = DrawViewportToolbar(gizmo);

                    _isHovered = ImGui.IsWindowHovered();
                    if (_isHovered && !toolbarHovered && isEditorMode)
                    {
                        var mousePos = ImGui.GetMousePos();
                        var localMouse = mousePos - imageScreenPos;
                        gizmo.Update(camera, selectedEntities, selected, localMouse, new Vector2(w, h));
                    }
                    else
                    {
                        gizmo.Update(camera, Array.Empty<Core.ECS.Entity>(), null, Vector2.Zero, Vector2.One);
                    }
                }

                // Gizmo drag batching for undo
                if (_app.Mode == EditorMode.Edit && gizmo.IsDragging && !_wasGizmoDragging)
                {
                    _app.UndoRedo.BeginBatch(_app.GetSelectedEntityIds());
                }
                else if (_app.Mode == EditorMode.Edit && !gizmo.IsDragging && _wasGizmoDragging)
                {
                    _app.Prefabs.RecalculateOverridesForScene();
                    _app.UndoRedo.EndBatch(_app.CurrentScene, _app.GetSelectedEntityIds());
                }
                _wasGizmoDragging = _app.Mode == EditorMode.Edit && gizmo.IsDragging;
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

        _app.EditorCamera.Update(Raylib.GetFrameTime(), _isHovered && _app.CanUseEditorViewportTools);
    }

    private unsafe void HandleAssetDropTarget(Camera3D camera, Vector2 imageScreenPos, int w, int h)
    {
        if (!ImGui.BeginDragDropTarget())
            return;

        // Peek to update live preview each frame while hovering
        var peekPayload = ImGui.AcceptDragDropPayload(AssetBrowserPanel.AssetDragPayload, ImGuiDragDropFlags.AcceptPeekOnly);
        if (peekPayload.NativePtr != null)
        {
            var assetPath = _app.DraggedAssetPath;
            if (!string.IsNullOrEmpty(assetPath))
            {
                var asset = AssetDatabase.Instance.GetAssets()
                    .FirstOrDefault(a => string.Equals(a.RelativePath, assetPath, StringComparison.OrdinalIgnoreCase));

                if (asset != null && asset.Type is AssetType.Prefab or AssetType.Model)
                {
                    var mousePos = ImGui.GetMousePos();
                    var localMouse = mousePos - imageScreenPos;
                    _dragPreviewPosition = ComputeDropWorldPosition(camera, localMouse, new Vector2(w, h));
                }
            }
        }

        // Accept delivery for the actual drop
        var payload = ImGui.AcceptDragDropPayload(AssetBrowserPanel.AssetDragPayload);
        if (payload.NativePtr != null && payload.Delivery)
        {
            var assetPath = _app.DraggedAssetPath;
            if (!string.IsNullOrEmpty(assetPath))
            {
                var asset = AssetDatabase.Instance.GetAssets()
                    .FirstOrDefault(a => string.Equals(a.RelativePath, assetPath, StringComparison.OrdinalIgnoreCase));

                if (asset != null)
                {
                    var mousePos = ImGui.GetMousePos();
                    var localMouse = mousePos - imageScreenPos;
                    HandleAssetDrop(asset, camera, localMouse, new Vector2(w, h));
                }
            }

            _dragPreviewPosition = null;
        }

        ImGui.EndDragDropTarget();
    }

    private void HandleAssetDrop(AssetEntry asset, Camera3D camera, Vector2 localMouse, Vector2 viewportSize)
    {
        var dropPos = ComputeDropWorldPosition(camera, localMouse, viewportSize);

        switch (asset.Type)
        {
            case AssetType.Prefab:
                _app.InstantiatePrefabAsset(asset.RelativePath);
                if (_app.SelectedEntity != null)
                    _app.SelectedEntity.Transform.LocalPosition = dropPos;
                break;

            case AssetType.Model:
                if (_app.CurrentScene == null || !_app.CanEditScene)
                    break;
                _app.RecordUndo();
                var name = Path.GetFileNameWithoutExtension(asset.FileName);
                var entity = _app.CurrentScene.CreateEntity(name);
                var meshRenderer = entity.AddComponent<MeshRendererComponent>();
                meshRenderer.ModelPath = asset.RelativePath;
                entity.Transform.LocalPosition = dropPos;
                _app.SetSingleSelection(entity);
                _app.RefreshUndoBaseline();
                NotificationManager.Instance.Post($"Created: {name}", NotificationType.Info, 1.5f);
                break;

            case AssetType.Script:
                HandleScriptDrop(asset, camera, localMouse, viewportSize);
                break;
        }
    }

    private void HandleScriptDrop(AssetEntry asset, Camera3D camera, Vector2 localMouse, Vector2 viewportSize)
    {
        if (_app.CurrentScene == null || !_app.CanEditScene)
            return;

        var pickedEntity = _app.PickingSystem.Pick(_app.CurrentScene, camera, localMouse, viewportSize);
        if (pickedEntity == null)
        {
            NotificationManager.Instance.Post("Drop a script onto an entity.", NotificationType.Warning);
            return;
        }

        var typeName = Path.GetFileNameWithoutExtension(asset.FileName);
        var componentType = ComponentTypeResolver.Resolve(typeName);
        if (componentType == null)
        {
            NotificationManager.Instance.Post("Build scripts first.", NotificationType.Warning);
            return;
        }

        if (pickedEntity.GetComponent(componentType) != null)
        {
            NotificationManager.Instance.Post($"{typeName} already exists on {pickedEntity.Name}.", NotificationType.Warning);
            return;
        }

        _app.RecordUndo();
        pickedEntity.AddComponent(componentType);
        _app.SetSingleSelection(pickedEntity);
        _app.RefreshUndoBaseline();
        NotificationManager.Instance.Post($"Added {typeName} to {pickedEntity.Name}", NotificationType.Info, 1.5f);
    }

    private static Vector3 ComputeDropWorldPosition(Camera3D camera, Vector2 localMouse, Vector2 viewportSize)
    {
        var ray = RaycastUtils.GetViewportRay(camera, localMouse, viewportSize);

        // Intersect with Y=0 ground plane
        if (MathF.Abs(ray.Direction.Y) > 1e-6f)
        {
            float t = -ray.Position.Y / ray.Direction.Y;
            if (t > 0)
                return ray.Position + ray.Direction * t;
        }

        // Fallback: 10 units in front of camera
        return ray.Position + ray.Direction * 10f;
    }

    private static void DrawDropPreview(System.Numerics.Vector3 pos)
    {
        var previewColor = new Color(255, 220, 50, 200);

        // Flat ring on Y=0 ground plane
        Raylib.DrawCircle3D(
            new System.Numerics.Vector3(pos.X, pos.Y, pos.Z),
            0.5f,
            new System.Numerics.Vector3(1, 0, 0),
            90f,
            previewColor);

        // Vertical line from ground to indicate placement point
        Raylib.DrawLine3D(
            new System.Numerics.Vector3(pos.X, pos.Y, pos.Z),
            new System.Numerics.Vector3(pos.X, pos.Y + 1.0f, pos.Z),
            previewColor);

        // Small cross on the ground
        const float crossSize = 0.3f;
        Raylib.DrawLine3D(
            new System.Numerics.Vector3(pos.X - crossSize, pos.Y, pos.Z),
            new System.Numerics.Vector3(pos.X + crossSize, pos.Y, pos.Z),
            previewColor);
        Raylib.DrawLine3D(
            new System.Numerics.Vector3(pos.X, pos.Y, pos.Z - crossSize),
            new System.Numerics.Vector3(pos.X, pos.Y, pos.Z + crossSize),
            previewColor);
    }

    private static bool DrawViewportToolbar(GizmoSystem gizmo)
    {
        ImGui.SetCursorPos(new Vector2(8, 8));
        bool anyHovered = false;

        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(6, 4));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(2, 0));

        // Gizmo mode buttons (T/R/S)
        anyHovered |= DrawToolbarToggle("T", gizmo.Mode == GizmoMode.Translate, () => gizmo.Mode = GizmoMode.Translate);
        ImGui.SameLine();
        anyHovered |= DrawToolbarToggle("R", gizmo.Mode == GizmoMode.Rotate, () => gizmo.Mode = GizmoMode.Rotate);
        ImGui.SameLine();
        anyHovered |= DrawToolbarToggle("S", gizmo.Mode == GizmoMode.Scale, () => gizmo.Mode = GizmoMode.Scale);

        // Separator
        ImGui.SameLine(0, 12);
        anyHovered |= DrawToolbarSeparator();

        // Space toggle (World/Local)
        ImGui.SameLine(0, 12);
        var spaceLabel = gizmo.Space == GizmoSpace.World ? "World" : "Local";
        if (ImGui.Button(spaceLabel))
            gizmo.Space = gizmo.Space == GizmoSpace.World ? GizmoSpace.Local : GizmoSpace.World;
        anyHovered |= ImGui.IsItemHovered();

        // Separator
        ImGui.SameLine(0, 12);
        anyHovered |= DrawToolbarSeparator();

        // Snap controls
        ImGui.SameLine(0, 12);
        anyHovered |= DrawSnapToggleAndPreset("Pos", ref gizmo.SnapTranslation, ref gizmo.TranslationSnapValue, GizmoSystem.TranslationSnapPresets);
        ImGui.SameLine(0, 8);
        anyHovered |= DrawSnapToggleAndPreset("Rot", ref gizmo.SnapRotation, ref gizmo.RotationSnapValue, GizmoSystem.RotationSnapPresets);
        ImGui.SameLine(0, 8);
        anyHovered |= DrawSnapToggleAndPreset("Scl", ref gizmo.SnapScale, ref gizmo.ScaleSnapValue, GizmoSystem.ScaleSnapPresets);

        // Separator
        ImGui.SameLine(0, 12);
        anyHovered |= DrawToolbarSeparator();

        // Multi-transform mode
        ImGui.SameLine(0, 12);
        var multiLabel = gizmo.MultiMode == MultiTransformMode.Independent ? "Independent" : "Relative";
        if (ImGui.Button(multiLabel))
            gizmo.MultiMode = gizmo.MultiMode == MultiTransformMode.Independent
                ? MultiTransformMode.Relative
                : MultiTransformMode.Independent;
        anyHovered |= ImGui.IsItemHovered();

        ImGui.PopStyleVar(2);

        return anyHovered;
    }

    private static bool DrawToolbarToggle(string label, bool active, Action onClick)
    {
        if (active)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, ImGui.GetColorU32(ImGuiCol.ButtonActive));
        }

        if (ImGui.Button(label))
            onClick();

        if (active)
            ImGui.PopStyleColor();

        return ImGui.IsItemHovered();
    }

    private static bool DrawToolbarSeparator()
    {
        var pos = ImGui.GetCursorScreenPos();
        var drawList = ImGui.GetWindowDrawList();
        float height = ImGui.GetFrameHeight();
        drawList.AddLine(
            new Vector2(pos.X, pos.Y),
            new Vector2(pos.X, pos.Y + height),
            ImGui.GetColorU32(ImGuiCol.Separator));
        ImGui.Dummy(new Vector2(1, height));
        return false;
    }

    private static bool DrawSnapToggleAndPreset(string label, ref bool snapEnabled, ref float snapValue, float[] presets)
    {
        bool hovered = false;

        // Toggle button
        bool wasEnabled = snapEnabled;
        if (wasEnabled)
            ImGui.PushStyleColor(ImGuiCol.Button, ImGui.GetColorU32(ImGuiCol.ButtonActive));
        if (ImGui.Button(label))
            snapEnabled = !snapEnabled;
        if (wasEnabled)
            ImGui.PopStyleColor();
        hovered |= ImGui.IsItemHovered();

        // Dropdown for presets
        ImGui.SameLine(0, 2);
        ImGui.PushItemWidth(56);
        var previewText = snapValue.ToString("G4");
        if (ImGui.BeginCombo($"##{label}Snap", previewText, ImGuiComboFlags.NoArrowButton))
        {
            foreach (var preset in presets)
            {
                bool selected = MathF.Abs(preset - snapValue) < 1e-6f;
                if (ImGui.Selectable(preset.ToString("G4"), selected))
                    snapValue = preset;
                if (selected)
                    ImGui.SetItemDefaultFocus();
            }
            ImGui.EndCombo();
        }
        hovered |= ImGui.IsItemHovered();
        ImGui.PopItemWidth();

        return hovered;
    }
  
    private PhysicsHitboxDrawMode ResolvePhysicsHitboxDrawMode()
    {
        if (_app.IsPhysicsHitboxPreviewEnabled)
            return PhysicsHitboxDrawMode.All;

        bool isDefaultEditorView = _app.CanUseEditorViewportTools && !_app.IsGameViewEnabled;
        return isDefaultEditorView ? PhysicsHitboxDrawMode.SelectedOnly : PhysicsHitboxDrawMode.Off;
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
        if (_selectionOutlinePostShader.Id == 0)
        {
            FrinkyLog.Error("Failed to load selection outline post shader.");
            _selectionOutlinePostShaderLoaded = false;
            return;
        }

        _texelSizeLoc = Raylib.GetShaderLocation(_selectionOutlinePostShader, "texelSize");
        _outlineColorLoc = Raylib.GetShaderLocation(_selectionOutlinePostShader, "outlineColor");
        _outlineWidthLoc = Raylib.GetShaderLocation(_selectionOutlinePostShader, "outlineWidth");
        _selectionOutlinePostShaderLoaded = true;
    }

    private void CompositeSelectionOutline(int width, int height)
    {
        if (!_selectionOutlinePostShaderLoaded || _selectionOutlinePostShader.Id == 0)
            return;

        Raylib.BeginTextureMode(_outlineCompositeTexture);
        Raylib.ClearBackground(new Color(0, 0, 0, 0));

        // Pass 1: copy lit scene as-is.
        DrawFullscreenTexture(_renderTexture.Texture, width, height);
        Rlgl.DrawRenderBatchActive();

        // Pass 2: draw outline overlay from mask using post shader.
        Raylib.BeginShaderMode(_selectionOutlinePostShader);

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

        DrawFullscreenTexture(_selectionMaskTexture.Texture, width, height);
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
