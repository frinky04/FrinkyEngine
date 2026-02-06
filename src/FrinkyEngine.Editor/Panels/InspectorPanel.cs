using FrinkyEngine.Core.ECS;
using FrinkyEngine.Core.Serialization;
using ImGuiNET;

namespace FrinkyEngine.Editor.Panels;

public class InspectorPanel
{
    private readonly EditorApplication _app;
    private string _componentSearch = string.Empty;

    public bool FocusNameField { get; set; }

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
                if (FocusNameField)
                {
                    ImGui.SetKeyboardFocusHere();
                    FocusNameField = false;
                }

                string name = entity.Name;
                if (ImGui.InputText("Name", ref name, 128))
                    entity.Name = name;
                if (ImGui.IsItemActivated())
                    _app.RecordUndo();
                if (ImGui.IsItemDeactivatedAfterEdit())
                    _app.RefreshUndoBaseline();

                bool active = entity.Active;
                ImGui.SameLine();
                if (ImGui.Checkbox("Active", ref active))
                {
                    _app.RecordUndo();
                    entity.Active = active;
                    _app.RefreshUndoBaseline();
                }

                ImGui.Separator();

                Component? componentToRemove = null;
                foreach (var component in entity.Components)
                {
                    var componentType = component.GetType();
                    bool isTransform = component is Core.Components.TransformComponent;

                    bool opened;
                    if (isTransform)
                    {
                        // TransformComponent cannot be removed — no close button
                        opened = ImGui.CollapsingHeader(componentType.Name,
                            ImGuiTreeNodeFlags.DefaultOpen);
                    }
                    else
                    {
                        bool visible = true;
                        opened = ImGui.CollapsingHeader(componentType.Name, ref visible,
                            ImGuiTreeNodeFlags.DefaultOpen);

                        if (!visible)
                            componentToRemove = component;
                    }

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

                if (componentToRemove != null)
                {
                    _app.RecordUndo();
                    entity.RemoveComponent(componentToRemove);
                    _app.RefreshUndoBaseline();
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
                        _app.RecordUndo();
                        entity.AddComponent(type);
                        _app.RefreshUndoBaseline();
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
                                _app.RecordUndo();
                                entity.AddComponent(type);
                                _app.RefreshUndoBaseline();
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
                                _app.RecordUndo();
                                entity.AddComponent(type);
                                _app.RefreshUndoBaseline();
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
