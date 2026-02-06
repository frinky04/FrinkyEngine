using FrinkyEngine.Core.ECS;
using FrinkyEngine.Core.Serialization;
using ImGuiNET;

namespace FrinkyEngine.Editor.Panels;

public class InspectorPanel
{
    private readonly EditorApplication _app;
    private string _componentSearch = string.Empty;

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
            _componentSearch = string.Empty;
        }

        if (ImGui.BeginPopup("AddComponent"))
        {
            // Auto-focus search bar on open
            if (ImGui.IsWindowAppearing())
                ImGui.SetKeyboardFocusHere();

            ImGui.InputTextWithHint("##search", "Search...", ref _componentSearch, 256);
            ImGui.Separator();

            var allTypes = ComponentTypeResolver.GetAllComponentTypes()
                .Where(t => t != typeof(Core.Components.TransformComponent))
                .Where(t => entity.GetComponent(t) == null)
                .ToList();

            var isSearching = !string.IsNullOrWhiteSpace(_componentSearch);

            if (isSearching)
            {
                // Flat filtered list with source tags
                var search = _componentSearch.Trim();
                foreach (var type in allTypes)
                {
                    if (!type.Name.Contains(search, System.StringComparison.OrdinalIgnoreCase))
                        continue;

                    var source = ComponentTypeResolver.GetAssemblySource(type);
                    if (ImGui.Selectable($"{type.Name}  [{source}]"))
                    {
                        entity.AddComponent(type);
                        ImGui.CloseCurrentPopup();
                    }

                    DrawBaseClassTooltip(type);
                }
            }
            else
            {
                // Grouped by Engine / Game
                var engineTypes = allTypes.Where(t => ComponentTypeResolver.GetAssemblySource(t) == "Engine").ToList();
                var gameTypes = allTypes.Where(t => ComponentTypeResolver.GetAssemblySource(t) != "Engine").ToList();

                if (engineTypes.Count > 0)
                {
                    if (ImGui.CollapsingHeader("Engine", ImGuiTreeNodeFlags.DefaultOpen))
                    {
                        foreach (var type in engineTypes)
                        {
                            if (ImGui.Selectable($"  {type.Name}"))
                            {
                                entity.AddComponent(type);
                                ImGui.CloseCurrentPopup();
                            }

                            DrawBaseClassTooltip(type);
                        }
                    }
                }

                if (gameTypes.Count > 0)
                {
                    if (ImGui.CollapsingHeader("Game", ImGuiTreeNodeFlags.DefaultOpen))
                    {
                        foreach (var type in gameTypes)
                        {
                            if (ImGui.Selectable($"  {type.Name}"))
                            {
                                entity.AddComponent(type);
                                ImGui.CloseCurrentPopup();
                            }

                            DrawBaseClassTooltip(type);
                        }
                    }
                }
            }

            ImGui.EndPopup();
        }
    }

    private static void DrawBaseClassTooltip(Type type)
    {
        if (ImGui.IsItemHovered() && type.BaseType != null && type.BaseType != typeof(Component))
        {
            ImGui.SetTooltip($"Extends {type.BaseType.Name}");
        }
    }
}
