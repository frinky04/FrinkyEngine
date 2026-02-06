using FrinkyEngine.Core.Components;
using FrinkyEngine.Core.ECS;
using ImGuiNET;

namespace FrinkyEngine.Editor.Panels;

public class HierarchyPanel
{
    private readonly EditorApplication _app;
    private Guid? _rangeAnchorId;

    public HierarchyPanel(EditorApplication app)
    {
        _app = app;
    }

    public void Draw()
    {
        if (ImGui.Begin("Hierarchy"))
        {
            if (_app.CurrentScene != null)
            {
                var hierarchyOrder = BuildHierarchyOrder(_app.CurrentScene);
                foreach (var entity in _app.CurrentScene.Entities)
                {
                    if (entity.Transform.Parent == null)
                        DrawEntityNode(entity, hierarchyOrder);
                }

                ImGui.Separator();

                if (ImGui.Button("Add Entity"))
                {
                    _app.RecordUndo();
                    var newEntity = _app.CurrentScene.CreateEntity("New Entity");
                    _app.SetSingleSelection(newEntity);
                    _rangeAnchorId = newEntity.Id;
                    _app.RefreshUndoBaseline();
                }

                if (ImGui.BeginPopupContextWindow("HierarchyContext", ImGuiPopupFlags.MouseButtonRight | ImGuiPopupFlags.NoOpenOverItems))
                {
                    if (ImGui.MenuItem("Create Empty"))
                    {
                        _app.RecordUndo();
                        var entity = _app.CurrentScene.CreateEntity("Empty");
                        _app.SetSingleSelection(entity);
                        _rangeAnchorId = entity.Id;
                        _app.RefreshUndoBaseline();
                    }
                    if (ImGui.MenuItem("Create Camera"))
                    {
                        _app.RecordUndo();
                        var cam = _app.CurrentScene.CreateEntity("Camera");
                        cam.AddComponent<CameraComponent>();
                        _app.SetSingleSelection(cam);
                        _rangeAnchorId = cam.Id;
                        _app.RefreshUndoBaseline();
                    }
                    if (ImGui.MenuItem("Create Light"))
                    {
                        _app.RecordUndo();
                        var light = _app.CurrentScene.CreateEntity("Light");
                        light.AddComponent<LightComponent>();
                        _app.SetSingleSelection(light);
                        _rangeAnchorId = light.Id;
                        _app.RefreshUndoBaseline();
                    }
                    ImGui.EndPopup();
                }
            }
        }
        ImGui.End();
    }

    private void DrawEntityNode(Entity entity, List<Entity> hierarchyOrder)
    {
        var flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanAvailWidth;
        if (_app.IsSelected(entity))
            flags |= ImGuiTreeNodeFlags.Selected;
        if (entity.Transform.Children.Count == 0)
            flags |= ImGuiTreeNodeFlags.Leaf;

        bool opened = ImGui.TreeNodeEx(entity.Id.ToString(), flags, entity.Name);

        if (ImGui.IsItemClicked())
            HandleEntitySelection(entity, hierarchyOrder);

        if (opened)
        {
            foreach (var child in entity.Transform.Children)
                DrawEntityNode(child.Entity, hierarchyOrder);
            ImGui.TreePop();
        }
    }

    private void HandleEntitySelection(Entity entity, List<Entity> hierarchyOrder)
    {
        var io = ImGui.GetIO();
        if (io.KeyShift)
        {
            var anchor = ResolveAnchorEntity(hierarchyOrder);
            if (anchor == null)
            {
                _app.SetSingleSelection(entity);
                _rangeAnchorId = entity.Id;
                return;
            }

            int anchorIndex = hierarchyOrder.FindIndex(e => e.Id == anchor.Id);
            int currentIndex = hierarchyOrder.FindIndex(e => e.Id == entity.Id);
            if (anchorIndex < 0 || currentIndex < 0)
            {
                _app.SetSingleSelection(entity);
                _rangeAnchorId = entity.Id;
                return;
            }

            int start = Math.Min(anchorIndex, currentIndex);
            int end = Math.Max(anchorIndex, currentIndex);
            var range = hierarchyOrder.Skip(start).Take(end - start + 1).ToList();
            _app.SetSelection(range);
            return;
        }

        if (io.KeyCtrl)
        {
            _app.ToggleSelection(entity);
            _rangeAnchorId = entity.Id;
            return;
        }

        _app.SetSingleSelection(entity);
        _rangeAnchorId = entity.Id;
    }

    private Entity? ResolveAnchorEntity(List<Entity> hierarchyOrder)
    {
        if (_rangeAnchorId.HasValue)
            return hierarchyOrder.FirstOrDefault(e => e.Id == _rangeAnchorId.Value);

        return _app.SelectedEntity;
    }

    private static List<Entity> BuildHierarchyOrder(Core.Scene.Scene scene)
    {
        var ordered = new List<Entity>();
        foreach (var entity in scene.Entities)
        {
            if (entity.Transform.Parent != null)
                continue;
            AppendEntityTree(entity, ordered);
        }

        return ordered;
    }

    private static void AppendEntityTree(Entity entity, List<Entity> ordered)
    {
        ordered.Add(entity);
        foreach (var child in entity.Transform.Children)
            AppendEntityTree(child.Entity, ordered);
    }
}
