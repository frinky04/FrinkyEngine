using FrinkyEngine.Core.ECS;
using FrinkyEngine.Core.Serialization;
using ImGuiNET;

namespace FrinkyEngine.Editor.Panels;

public class InspectorPanel
{
    private readonly EditorApplication _app;

    public InspectorPanel(EditorApplication app)
    {
        _app = app;
    }

    public void Draw()
    {
        if (ImGui.Begin("Inspector"))
        {
            var entity = _app.SelectedEntity;
            if (entity != null)
            {
                string name = entity.Name;
                if (ImGui.InputText("Name", ref name, 128))
                    entity.Name = name;

                bool active = entity.Active;
                ImGui.SameLine();
                if (ImGui.Checkbox("Active", ref active))
                    entity.Active = active;

                ImGui.Separator();

                foreach (var component in entity.Components)
                {
                    var componentType = component.GetType();
                    bool enabled = component.Enabled;
                    bool opened = ImGui.CollapsingHeader(componentType.Name, ref enabled,
                        ImGuiTreeNodeFlags.DefaultOpen);

                    if (enabled != component.Enabled)
                        component.Enabled = enabled;

                    if (opened)
                    {
                        ImGui.PushID(componentType.Name);
                        if (!ComponentDrawerRegistry.Draw(component))
                        {
                            ComponentDrawerRegistry.DrawReflection(component);
                        }
                        ImGui.PopID();
                    }
                }

                ImGui.Separator();
                DrawAddComponentButton(entity);
            }
            else
            {
                ImGui.TextDisabled("No entity selected.");
            }
        }
        ImGui.End();
    }

    private void DrawAddComponentButton(Entity entity)
    {
        if (ImGui.Button("Add Component", new System.Numerics.Vector2(-1, 0)))
        {
            ImGui.OpenPopup("AddComponent");
        }

        if (ImGui.BeginPopup("AddComponent"))
        {
            foreach (var type in ComponentTypeResolver.GetAllComponentTypes())
            {
                if (type == typeof(Core.Components.TransformComponent)) continue;
                if (entity.GetComponent(type) != null) continue;

                if (ImGui.Selectable(type.Name))
                {
                    entity.AddComponent(type);
                }
            }
            ImGui.EndPopup();
        }
    }
}
