using System.Numerics;
using System.Reflection;
using FrinkyEngine.Core.Components;
using FrinkyEngine.Core.ECS;
using FrinkyEngine.Core.Serialization;
using ImGuiNET;
using Raylib_cs;

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
            DrawInspectorContents();
        }
        ImGui.End();
    }

    private void DrawInspectorContents()
    {
        var selectedEntities = _app.SelectedEntities
            .Where(e => e.Scene == _app.CurrentScene)
            .ToList();

        if (selectedEntities.Count == 0)
        {
            FocusNameField = false;
            ImGui.TextDisabled("No entity selected.");
            return;
        }

        if (selectedEntities.Count == 1)
        {
            DrawSingleEntityInspector(selectedEntities[0]);
            return;
        }

        DrawMultiEntityInspector(selectedEntities);
    }

    private void DrawSingleEntityInspector(Entity entity)
    {
        DrawPrefabHeader(entity);

        if (FocusNameField)
        {
            ImGui.SetKeyboardFocusHere();
            FocusNameField = false;
        }

        string name = entity.Name;
        if (ImGui.InputText("Name", ref name, 128))
            entity.Name = name;
        TrackContinuousUndo();

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
            bool isTransform = component is TransformComponent;

            bool opened;
            if (isTransform)
            {
                opened = ImGui.CollapsingHeader(componentType.Name, ImGuiTreeNodeFlags.DefaultOpen);
            }
            else
            {
                bool visible = true;
                opened = ImGui.CollapsingHeader(componentType.Name, ref visible, ImGuiTreeNodeFlags.DefaultOpen);
                if (!visible)
                    componentToRemove = component;
            }

            if (opened)
            {
                ImGui.PushID(componentType.Name);
                if (!ComponentDrawerRegistry.Draw(component))
                    ComponentDrawerRegistry.DrawReflection(component);
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
        DrawAddComponentButton(new[] { entity });
    }

    private void DrawPrefabHeader(Entity entity)
    {
        var root = _app.Prefabs.GetPrefabRoot(entity);
        if (root?.Prefab == null)
            return;

        var metadata = root.Prefab;
        bool isRoot = root.Id == entity.Id;

        ImGui.TextDisabled($"Prefab: {metadata.AssetPath}");
        int overrideCount = (metadata.Overrides?.PropertyOverrides.Count ?? 0)
                            + (metadata.Overrides?.AddedComponents.Count ?? 0)
                            + (metadata.Overrides?.RemovedComponents.Count ?? 0)
                            + (metadata.Overrides?.AddedChildren.Count ?? 0)
                            + (metadata.Overrides?.RemovedChildren.Count ?? 0);
        ImGui.TextDisabled($"Overrides: {overrideCount}");
        DrawPrefabOverrideDetails(metadata.Overrides, overrideCount);

        if (!isRoot)
        {
            if (ImGui.SmallButton("Select Prefab Root"))
                _app.SetSingleSelection(root);
            ImGui.Separator();
            return;
        }

        if (ImGui.Button("Apply Prefab"))
            _app.ApplySelectedPrefab();

        ImGui.SameLine();
        if (ImGui.Button("Revert Prefab"))
            _app.RevertSelectedPrefab();

        ImGui.SameLine();
        if (ImGui.Button("Make Unique"))
            _app.MakeUniqueSelectedPrefab();

        ImGui.SameLine();
        if (ImGui.Button("Unpack"))
            _app.UnpackSelectedPrefab();

        ImGui.Separator();
    }

    private static void DrawPrefabOverrideDetails(Core.Prefabs.PrefabOverridesData? overrides, int overrideCount)
    {
        if (overrideCount <= 0 || overrides == null)
            return;

        if (!ImGui.TreeNode($"Override Details ({overrideCount})"))
            return;

        foreach (var item in overrides.PropertyOverrides)
            ImGui.TextDisabled($"Property: {item.ComponentType}.{item.PropertyName} @ {item.NodeId}");

        foreach (var item in overrides.AddedComponents)
            ImGui.TextDisabled($"Added Component: {item.Component.Type} @ {item.NodeId}");

        foreach (var item in overrides.RemovedComponents)
            ImGui.TextDisabled($"Removed Component: {item.ComponentType} @ {item.NodeId}");

        foreach (var item in overrides.AddedChildren)
            ImGui.TextDisabled($"Added Child: {item.Child.Name} @ {item.ParentNodeId}");

        foreach (var item in overrides.RemovedChildren)
            ImGui.TextDisabled($"Removed Child: {item}");

        ImGui.TreePop();
    }

    private void DrawMultiEntityInspector(IReadOnlyList<Entity> entities)
    {
        FocusNameField = false;
        ImGui.TextDisabled($"{entities.Count} entities selected");

        bool firstActive = entities[0].Active;
        bool mixedActive = entities.Any(e => e.Active != firstActive);
        bool active = firstActive;
        var activeLabel = GetMixedLabel("Active", mixedActive);
        if (ImGui.Checkbox(activeLabel, ref active))
        {
            _app.RecordUndo();
            foreach (var entity in entities)
                entity.Active = active;
            _app.RefreshUndoBaseline();
        }

        ImGui.Separator();

        var primary = _app.SelectedEntity ?? entities[^1];
        var commonComponentTypes = GetCommonComponentTypes(primary, entities);
        Type? componentTypeToRemove = null;

        foreach (var componentType in commonComponentTypes)
        {
            bool isTransform = componentType == typeof(TransformComponent);
            bool opened;
            if (isTransform)
            {
                opened = ImGui.CollapsingHeader(componentType.Name, ImGuiTreeNodeFlags.DefaultOpen);
            }
            else
            {
                bool visible = true;
                opened = ImGui.CollapsingHeader(componentType.Name, ref visible, ImGuiTreeNodeFlags.DefaultOpen);
                if (!visible)
                    componentTypeToRemove = componentType;
            }

            if (!opened)
                continue;

            var components = entities
                .Select(entity => entity.GetComponent(componentType))
                .OfType<Component>()
                .ToList();
            if (components.Count != entities.Count)
                continue;

            ImGui.PushID(componentType.FullName);
            if (!DrawMultiBuiltInComponent(componentType, components))
                DrawMultiReflection(components);
            ImGui.PopID();
        }

        if (componentTypeToRemove != null)
        {
            _app.RecordUndo();
            foreach (var entity in entities)
            {
                var component = entity.GetComponent(componentTypeToRemove);
                if (component != null)
                    entity.RemoveComponent(component);
            }
            _app.RefreshUndoBaseline();
        }

        ImGui.Separator();
        DrawAddComponentButton(entities);
    }

    private List<Type> GetCommonComponentTypes(Entity primary, IReadOnlyList<Entity> entities)
    {
        var orderedTypes = new List<Type>();
        foreach (var component in primary.Components)
        {
            var type = component.GetType();
            if (orderedTypes.Contains(type))
                continue;
            if (entities.All(entity => entity.Components.Any(c => c.GetType() == type)))
                orderedTypes.Add(type);
        }

        return orderedTypes;
    }

    private bool DrawMultiBuiltInComponent(Type componentType, IReadOnlyList<Component> components)
    {
        if (componentType == typeof(TransformComponent))
        {
            var transforms = components.Cast<TransformComponent>().ToList();
            DrawMultiTransform(transforms);
            return true;
        }

        return false;
    }

    private void DrawMultiTransform(IReadOnlyList<TransformComponent> transforms)
    {
        var localPositions = transforms.Select(t => t.LocalPosition).ToList();
        if (DrawMixedVector3("Position", localPositions[0], localPositions.Any(v => v != localPositions[0]), 0.1f, out var newPosition))
        {
            foreach (var transform in transforms)
                transform.LocalPosition = newPosition;
        }

        var eulerRotations = transforms.Select(t => t.EulerAngles).ToList();
        if (DrawMixedVector3("Rotation", eulerRotations[0], eulerRotations.Any(v => v != eulerRotations[0]), 0.5f, out var newEuler))
        {
            foreach (var transform in transforms)
                transform.EulerAngles = newEuler;
        }

        var scales = transforms.Select(t => t.LocalScale).ToList();
        if (DrawMixedVector3("Scale", scales[0], scales.Any(v => v != scales[0]), 0.05f, out var newScale))
        {
            foreach (var transform in transforms)
                transform.LocalScale = newScale;
        }
    }

    private void DrawMultiReflection(IReadOnlyList<Component> components)
    {
        var type = components[0].GetType();
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanRead || !prop.CanWrite) continue;
            if (prop.Name is "Entity" or "HasStarted" or "Enabled") continue;
            if (prop.Name is "RenderModel") continue;

            DrawMultiProperty(components, prop);
        }
    }

    private void DrawMultiProperty(IReadOnlyList<Component> components, PropertyInfo prop)
    {
        var propType = prop.PropertyType;
        var label = prop.Name;
        var firstValue = prop.GetValue(components[0]);
        bool mixed = components.Skip(1).Any(c => !Equals(prop.GetValue(c), firstValue));

        if (propType == typeof(float))
        {
            float val = firstValue is float f ? f : 0f;
            if (DrawMixedDragFloat(label, ref val, mixed, 0.1f))
            {
                foreach (var component in components)
                    prop.SetValue(component, val);
            }
        }
        else if (propType == typeof(int))
        {
            int val = firstValue is int i ? i : 0;
            if (DrawMixedDragInt(label, ref val, mixed))
            {
                foreach (var component in components)
                    prop.SetValue(component, val);
            }
        }
        else if (propType == typeof(bool))
        {
            bool val = firstValue is bool b && b;
            if (ImGui.Checkbox(GetMixedLabel(label, mixed), ref val))
            {
                _app.RecordUndo();
                foreach (var component in components)
                    prop.SetValue(component, val);
                _app.RefreshUndoBaseline();
            }
        }
        else if (propType == typeof(string))
        {
            string val = firstValue as string ?? string.Empty;
            if (DrawMixedInputText(label, ref val, mixed, 256))
            {
                foreach (var component in components)
                    prop.SetValue(component, val);
            }
        }
        else if (propType == typeof(Vector3))
        {
            var val = firstValue is Vector3 v ? v : Vector3.Zero;
            if (DrawMixedVector3(label, val, mixed, 0.1f, out var updated))
            {
                foreach (var component in components)
                    prop.SetValue(component, updated);
            }
        }
        else if (propType == typeof(Vector2))
        {
            var val = firstValue is Vector2 v ? v : Vector2.Zero;
            if (DrawMixedVector2(label, val, mixed, 0.1f, out var updated))
            {
                foreach (var component in components)
                    prop.SetValue(component, updated);
            }
        }
        else if (propType == typeof(Quaternion))
        {
            var q = firstValue is Quaternion quaternion ? quaternion : Quaternion.Identity;
            var euler = Core.FrinkyMath.QuaternionToEuler(q);
            if (DrawMixedVector3(label, euler, mixed, 0.5f, out var updatedEuler))
            {
                var updatedQuaternion = Core.FrinkyMath.EulerToQuaternion(updatedEuler);
                foreach (var component in components)
                    prop.SetValue(component, updatedQuaternion);
            }
        }
        else if (propType == typeof(Color))
        {
            var colorValue = firstValue is Color color ? color : new Color(255, 255, 255, 255);
            var vec4 = ColorToVec4(colorValue);
            if (DrawMixedColor4(label, vec4, mixed, out var updatedColor))
            {
                var resolvedColor = Vec4ToColor(updatedColor);
                foreach (var component in components)
                    prop.SetValue(component, resolvedColor);
            }
        }
        else if (propType.IsEnum)
        {
            var names = Enum.GetNames(propType);
            int val = firstValue != null ? (int)Convert.ChangeType(firstValue, typeof(int)) : 0;
            if (ImGui.Combo(GetMixedLabel(label, mixed), ref val, names, names.Length))
            {
                _app.RecordUndo();
                var enumValue = Enum.ToObject(propType, val);
                foreach (var component in components)
                    prop.SetValue(component, enumValue);
                _app.RefreshUndoBaseline();
            }
        }
        else
        {
            ImGui.LabelText(label, propType.Name);
        }
    }

    private bool DrawMixedDragFloat(string label, ref float value, bool mixed, float speed)
    {
        bool changed = ImGui.DragFloat(GetMixedLabel(label, mixed), ref value, speed);
        TrackContinuousUndo();
        return changed;
    }

    private bool DrawMixedDragInt(string label, ref int value, bool mixed)
    {
        bool changed = ImGui.DragInt(GetMixedLabel(label, mixed), ref value);
        TrackContinuousUndo();
        return changed;
    }

    private bool DrawMixedInputText(string label, ref string value, bool mixed, uint maxLength)
    {
        bool changed = ImGui.InputText(GetMixedLabel(label, mixed), ref value, maxLength);
        TrackContinuousUndo();
        return changed;
    }

    private bool DrawMixedVector3(string label, Vector3 initial, bool mixed, float speed, out Vector3 value)
    {
        value = initial;
        bool changed = ImGui.DragFloat3(GetMixedLabel(label, mixed), ref value, speed);
        TrackContinuousUndo();
        return changed;
    }

    private bool DrawMixedVector2(string label, Vector2 initial, bool mixed, float speed, out Vector2 value)
    {
        value = initial;
        bool changed = ImGui.DragFloat2(GetMixedLabel(label, mixed), ref value, speed);
        TrackContinuousUndo();
        return changed;
    }

    private bool DrawMixedColor4(string label, Vector4 initial, bool mixed, out Vector4 value)
    {
        value = initial;
        bool changed = ImGui.ColorEdit4(GetMixedLabel(label, mixed), ref value);
        TrackContinuousUndo();
        return changed;
    }

    private void TrackContinuousUndo()
    {
        if (ImGui.IsItemActivated())
            _app.RecordUndo();
        if (ImGui.IsItemDeactivatedAfterEdit())
            _app.RefreshUndoBaseline();
    }

    private static Vector4 ColorToVec4(Color c) =>
        new(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);

    private static Color Vec4ToColor(Vector4 v) =>
        new((byte)(v.X * 255), (byte)(v.Y * 255), (byte)(v.Z * 255), (byte)(v.W * 255));

    private static string GetMixedLabel(string label, bool mixed)
    {
        return mixed ? $"{label} (Mixed)" : label;
    }

    private void DrawAddComponentButton(IReadOnlyList<Entity> entities)
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
                .Where(t => t != typeof(TransformComponent))
                .Where(t => entities.Any(entity => entity.GetComponent(t) == null))
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
                        AddComponentToEntities(entities, type);
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
                                AddComponentToEntities(entities, type);
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
                                AddComponentToEntities(entities, type);
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

    private void AddComponentToEntities(IReadOnlyList<Entity> entities, Type type)
    {
        _app.RecordUndo();
        foreach (var entity in entities)
        {
            if (entity.GetComponent(type) == null)
                entity.AddComponent(type);
        }
        _app.RefreshUndoBaseline();
    }

    private static void DrawBaseClassTooltip(Type type)
    {
        if (ImGui.IsItemHovered() && type.BaseType != null && type.BaseType != typeof(Component))
        {
            ImGui.SetTooltip($"Extends {type.BaseType.Name}");
        }
    }
}
