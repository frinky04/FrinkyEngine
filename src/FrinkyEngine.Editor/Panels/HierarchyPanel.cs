using FrinkyEngine.Core.Components;
using FrinkyEngine.Core.ECS;
using ImGuiNET;

namespace FrinkyEngine.Editor.Panels;

public class HierarchyPanel
{
    private readonly EditorApplication _app;

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
                foreach (var entity in _app.CurrentScene.Entities)
                {
                    if (entity.Transform.Parent == null)
                        DrawEntityNode(entity);
                }

                ImGui.Separator();

                if (ImGui.Button("Add Entity"))
                {
                    _app.RecordUndo();
                    var newEntity = _app.CurrentScene.CreateEntity("New Entity");
                    _app.SelectedEntity = newEntity;
                    _app.RefreshUndoBaseline();
                }

                if (ImGui.BeginPopupContextWindow("HierarchyContext", ImGuiPopupFlags.MouseButtonRight | ImGuiPopupFlags.NoOpenOverItems))
                {
                    if (ImGui.MenuItem("Create Empty"))
                    {
                        _app.RecordUndo();
                        _app.SelectedEntity = _app.CurrentScene.CreateEntity("Empty");
                        _app.RefreshUndoBaseline();
                    }
                    if (ImGui.MenuItem("Create Camera"))
                    {
                        _app.RecordUndo();
                        var cam = _app.CurrentScene.CreateEntity("Camera");
                        cam.AddComponent<CameraComponent>();
                        _app.SelectedEntity = cam;
                        _app.RefreshUndoBaseline();
                    }
                    if (ImGui.MenuItem("Create Light"))
                    {
                        _app.RecordUndo();
                        var light = _app.CurrentScene.CreateEntity("Light");
                        light.AddComponent<LightComponent>();
                        _app.SelectedEntity = light;
                        _app.RefreshUndoBaseline();
                    }
                    ImGui.EndPopup();
                }
            }
        }
        ImGui.End();
    }

    private void DrawEntityNode(Entity entity)
    {
        var flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanAvailWidth;
        if (_app.SelectedEntity == entity)
            flags |= ImGuiTreeNodeFlags.Selected;
        if (entity.Transform.Children.Count == 0)
            flags |= ImGuiTreeNodeFlags.Leaf;

        bool opened = ImGui.TreeNodeEx(entity.Id.ToString(), flags, entity.Name);

        if (ImGui.IsItemClicked())
            _app.SelectedEntity = entity;

        if (opened)
        {
            foreach (var child in entity.Transform.Children)
                DrawEntityNode(child.Entity);
            ImGui.TreePop();
        }
    }
}
