using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.Components;
using FrinkyEngine.Core.ECS;
using FrinkyEngine.Core.Serialization;
using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Widgets;
using Raylib_cs;
using FrinkyEngine.Core.Scene;

namespace FrinkyEngine.Editor.Panels;

public class InspectorPanel
{
    private readonly EditorApplication _app;
    private string _componentSearch = string.Empty;
    private static readonly Dictionary<string, string> _enumSearchFilters = new();

    public bool FocusNameField { get; set; }

    public InspectorPanel(EditorApplication app)
    {
        _app = app;
    }

    public void Draw()
    {
        if (ImGui.Begin("Inspector"))
        {
            if (!_app.CanEditScene)
                ImGui.TextDisabled("Editing is disabled in Play mode.");

            ImGui.BeginDisabled(!_app.CanEditScene);
            DrawInspectorContents();
            ImGui.EndDisabled();
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
        ImGui.SameLine(0.0f, 8.0f);
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
            var displayName = ComponentTypeResolver.GetDisplayName(componentType);
            bool isTransform = component is TransformComponent;

            bool opened;
            if (isTransform)
            {
                opened = ImGui.CollapsingHeader(displayName, ImGuiTreeNodeFlags.DefaultOpen);
            }
            else
            {
                bool visible = true;
                opened = ImGui.CollapsingHeader(displayName, ref visible, ImGuiTreeNodeFlags.DefaultOpen);
                if (!visible)
                    componentToRemove = component;
            }

            if (opened)
            {
                ImGui.PushID(componentType.Name);
                ComponentDrawerRegistry.DrawReflection(component);
                if (component is SkinnedMeshAnimatorComponent animatorComponent)
                    DrawBoneHierarchyTree(animatorComponent, entity);
                ImGui.PopID();
            }
        }

        if (componentToRemove != null)
        {
            _app.RecordUndo();
            entity.RemoveComponent(componentToRemove);
            _app.RefreshUndoBaseline();
        }

        // Draw unresolved components (old/renamed/deleted types preserved as raw JSON)
        if (entity.HasUnresolvedComponents)
        {
            DrawUnresolvedComponents(entity);
        }

        ImGui.Separator();
        DrawAddComponentButton(new[] { entity });
    }

    private void DrawUnresolvedComponents(Entity entity)
    {
        ImGui.Separator();
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.7f, 0.3f, 1.0f));
        ImGui.TextWrapped($"{entity.UnresolvedComponents.Count} unresolved component(s)");
        ImGui.PopStyleColor();

        ComponentData? unresolvedToRemove = null;
        for (int i = 0; i < entity.UnresolvedComponents.Count; i++)
        {
            var data = entity.UnresolvedComponents[i];
            ImGui.PushID($"unresolved_{i}");

            // Extract just the class name from the fully qualified type name for display
            var typeName = data.Type;
            var lastDot = typeName.LastIndexOf('.');
            var shortName = lastDot >= 0 ? typeName[(lastDot + 1)..] : typeName;

            bool opened = ImGui.TreeNode($"{shortName} (unresolved)");
            ImGui.SameLine();
            if (ImGui.SmallButton("Remove"))
                unresolvedToRemove = data;

            if (opened)
            {
                ImGui.TextDisabled($"Type: {typeName}");
                ImGui.TextDisabled($"Properties: {data.Properties.Count}");
                foreach (var (propName, _) in data.Properties)
                    ImGui.BulletText(propName);
                ImGui.TreePop();
            }

            ImGui.PopID();
        }

        if (unresolvedToRemove != null)
        {
            _app.RecordUndo();
            entity.RemoveUnresolvedComponent(unresolvedToRemove);
            _app.RefreshUndoBaseline();
        }

        if (entity.UnresolvedComponents.Count > 1)
        {
            if (ImGui.Button("Remove All Unresolved"))
            {
                _app.RecordUndo();
                entity.ClearUnresolvedComponents();
                _app.RefreshUndoBaseline();
            }
        }
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
            var displayName = ComponentTypeResolver.GetDisplayName(componentType);
            bool isTransform = componentType == typeof(TransformComponent);
            bool opened;
            if (isTransform)
            {
                opened = ImGui.CollapsingHeader(displayName, ImGuiTreeNodeFlags.DefaultOpen);
            }
            else
            {
                bool visible = true;
                opened = ImGui.CollapsingHeader(displayName, ref visible, ImGuiTreeNodeFlags.DefaultOpen);
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

    private string? _lastMultiHeader;

    private void DrawMultiReflection(IReadOnlyList<Component> components)
    {
        string? lastSection = null;
        _lastMultiHeader = null;
        var type = components[0].GetType();
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!IsInspectableProperty(prop))
                continue;
            if (!IsPropertyVisibleForAll(components, prop))
                continue;

            ApplyMultiLayoutAttributes(prop, ref lastSection);
            DrawMultiProperty(components, prop);
            EndMultiLayoutAttributes(prop);
        }
    }

    private void ApplyMultiLayoutAttributes(PropertyInfo prop, ref string? lastSection)
    {
        var spaceAttr = prop.GetCustomAttribute<InspectorSpaceAttribute>();
        if (spaceAttr != null)
            ImGui.Dummy(new Vector2(0, spaceAttr.Height));

        var section = prop.GetCustomAttribute<InspectorSectionAttribute>()?.Title;
        if (!string.IsNullOrWhiteSpace(section) && !string.Equals(section, lastSection, StringComparison.Ordinal))
        {
            ImGui.SeparatorText(section);
            lastSection = section;
        }

        var headerAttr = prop.GetCustomAttribute<InspectorHeaderAttribute>();
        if (headerAttr != null && !string.Equals(headerAttr.Title, _lastMultiHeader, StringComparison.Ordinal))
        {
            ImGui.TextDisabled(headerAttr.Title);
            _lastMultiHeader = headerAttr.Title;
        }

        var indentAttr = prop.GetCustomAttribute<InspectorIndentAttribute>();
        if (indentAttr != null)
            ImGui.Indent(indentAttr.Levels * 16f);
    }

    private void EndMultiLayoutAttributes(PropertyInfo prop)
    {
        var indentAttr = prop.GetCustomAttribute<InspectorIndentAttribute>();
        if (indentAttr != null)
            ImGui.Unindent(indentAttr.Levels * 16f);
    }

    private void DrawMultiPropertyWithTooltip(PropertyInfo prop, Action drawAction)
    {
        var tooltipAttr = prop.GetCustomAttribute<InspectorTooltipAttribute>();
        if (tooltipAttr != null)
            ImGui.BeginGroup();

        drawAction();

        if (tooltipAttr != null)
        {
            ImGui.EndGroup();
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip(tooltipAttr.Tooltip);
        }
    }

    private void DrawMultiProperty(IReadOnlyList<Component> components, PropertyInfo prop)
    {
        var propType = prop.PropertyType;
        var label = GetInspectorLabel(prop);
        var firstValue = prop.GetValue(components[0]);
        bool mixed = components.Skip(1).Any(c => !Equals(prop.GetValue(c), firstValue));
        bool isReadOnly = !prop.CanWrite || prop.GetCustomAttribute<InspectorReadOnlyAttribute>() != null;

        if (isReadOnly)
        {
            DrawMultiReadOnlyValue(label, firstValue, mixed);
            return;
        }

        var rangeAttr = prop.GetCustomAttribute<InspectorRangeAttribute>();
        void SetAll(object? value)
        {
            foreach (var component in components)
            {
                prop.SetValue(component, value);
                ComponentDrawerRegistry.InvokeOnChangedCallbacks(component, prop);
            }
        }

        if (propType == typeof(float))
        {
            float val = firstValue is float f ? f : 0f;
            float speed = rangeAttr?.Speed ?? 0.1f;
            float min = rangeAttr?.Min ?? float.MinValue;
            float max = rangeAttr?.Max ?? float.MaxValue;
            DrawMultiPropertyWithTooltip(prop, () =>
            {
                if (DrawMixedDragFloat(label, ref val, mixed, speed, min, max))
                    SetAll(val);
            });
        }
        else if (propType == typeof(int))
        {
            int val = firstValue is int i ? i : 0;
            float speed = rangeAttr?.Speed ?? 1f;
            int min = (int)(rangeAttr?.Min ?? int.MinValue);
            int max = (int)(rangeAttr?.Max ?? int.MaxValue);
            DrawMultiPropertyWithTooltip(prop, () =>
            {
                if (DrawMixedDragInt(label, ref val, mixed, speed, min, max))
                    SetAll(val);
            });
        }
        else if (propType == typeof(bool))
        {
            bool val = firstValue is bool b && b;
            DrawMultiPropertyWithTooltip(prop, () =>
            {
                if (ImGui.Checkbox(GetMixedLabel(label, mixed), ref val))
                {
                    _app.RecordUndo();
                    SetAll(val);
                    _app.RefreshUndoBaseline();
                }
            });
        }
        else if (propType == typeof(string))
        {
            string val = firstValue as string ?? string.Empty;
            DrawMultiPropertyWithTooltip(prop, () =>
            {
                if (DrawMixedInputText(label, ref val, mixed, 256))
                    SetAll(val);
            });
        }
        else if (propType == typeof(Vector3))
        {
            var val = firstValue is Vector3 v ? v : Vector3.Zero;
            DrawMultiPropertyWithTooltip(prop, () =>
            {
                if (DrawMixedVector3(label, val, mixed, 0.1f, out var updated))
                    SetAll(updated);
            });
        }
        else if (propType == typeof(Vector2))
        {
            var val = firstValue is Vector2 v ? v : Vector2.Zero;
            DrawMultiPropertyWithTooltip(prop, () =>
            {
                if (DrawMixedVector2(label, val, mixed, 0.1f, out var updated))
                    SetAll(updated);
            });
        }
        else if (propType == typeof(Quaternion))
        {
            var q = firstValue is Quaternion quaternion ? quaternion : Quaternion.Identity;
            var euler = Core.FrinkyMath.QuaternionToEuler(q);
            DrawMultiPropertyWithTooltip(prop, () =>
            {
                if (DrawMixedVector3(label, euler, mixed, 0.5f, out var updatedEuler))
                {
                    var updatedQuaternion = Core.FrinkyMath.EulerToQuaternion(updatedEuler);
                    SetAll(updatedQuaternion);
                }
            });
        }
        else if (propType == typeof(Color))
        {
            var colorValue = firstValue is Color color ? color : new Color(255, 255, 255, 255);
            var vec4 = ColorToVec4(colorValue);
            DrawMultiPropertyWithTooltip(prop, () =>
            {
                if (DrawMixedColor4(label, vec4, mixed, out var updatedColor))
                    SetAll(Vec4ToColor(updatedColor));
            });
        }
        else if (propType.IsEnum)
        {
            object currentValue = firstValue ?? Enum.GetValues(propType).GetValue(0)!;
            DrawMultiPropertyWithTooltip(prop, () =>
            {
                bool changed = prop.GetCustomAttribute<InspectorSearchableEnumAttribute>() != null
                    ? DrawMixedSearchableEnum(label, propType, ref currentValue, mixed)
                    : ComboEnumHelper.Combo(GetMixedLabel(label, mixed), propType, ref currentValue);
                if (changed)
                {
                    _app.RecordUndo();
                    SetAll(currentValue);
                    _app.RefreshUndoBaseline();
                }
            });
        }
        else if (propType == typeof(EntityReference))
        {
            var entityRef = firstValue is EntityReference er ? er : EntityReference.None;
            var scene = _app.CurrentScene;
            var resolved = entityRef.IsValid ? scene?.FindEntityById(entityRef.Id) : null;
            string preview = mixed ? "(Mixed)" : (!entityRef.IsValid ? "(None)" : (resolved?.Name ?? "(Missing)"));
            ImGui.LabelText(label, preview);
        }
        else if (propType == typeof(AssetReference)
                 || typeof(FObject).IsAssignableFrom(propType)
                 || IsFObjectListType(propType)
                 || IsListType(propType)
                 || IsInlineObjectType(propType))
        {
            ImGui.LabelText(label, "(edit individually)");
        }
        else
        {
            ImGui.LabelText(label, propType.Name);
        }
    }

    private static bool IsFObjectListType(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            var arg = type.GetGenericArguments()[0];
            return typeof(FObject).IsAssignableFrom(arg);
        }
        return false;
    }

    private static bool IsListType(Type type)
    {
        return InspectorReflectionHelpers.IsListType(type);
    }

    private static bool IsInlineObjectType(Type type)
    {
        return InspectorReflectionHelpers.IsInlineObjectType(type);
    }

    private static bool IsInspectableProperty(PropertyInfo prop)
    {
        return InspectorReflectionHelpers.IsInspectableComponentProperty(prop);
    }

    private static bool IsPropertyVisibleForAll(IReadOnlyList<Component> components, PropertyInfo prop)
    {
        foreach (var visibleIf in prop.GetCustomAttributes<InspectorVisibleIfAttribute>())
        {
            if (!InspectorReflectionHelpers.TryEvaluateBoolMember(components[0], visibleIf.PropertyName, out var firstValue))
                return false;
            if (firstValue != visibleIf.ExpectedValue)
                return false;

            foreach (var component in components)
            {
                if (!InspectorReflectionHelpers.TryEvaluateBoolMember(component, visibleIf.PropertyName, out var current)
                    || current != visibleIf.ExpectedValue)
                    return false;
            }
        }

        foreach (var visibleIfEnum in prop.GetCustomAttributes<InspectorVisibleIfEnumAttribute>())
        {
            if (!InspectorReflectionHelpers.TryEvaluateEnumMember(components[0], visibleIfEnum.PropertyName, out var firstValue))
                return false;
            if (!string.Equals(firstValue?.ToString(), visibleIfEnum.ExpectedMemberName, StringComparison.Ordinal))
                return false;

            foreach (var component in components)
            {
                if (!InspectorReflectionHelpers.TryEvaluateEnumMember(component, visibleIfEnum.PropertyName, out var enumValue))
                    return false;
                if (!string.Equals(enumValue?.ToString(), visibleIfEnum.ExpectedMemberName, StringComparison.Ordinal))
                    return false;
            }
        }

        return true;
    }

    private static string GetInspectorLabel(PropertyInfo prop)
    {
        var label = prop.GetCustomAttribute<InspectorLabelAttribute>()?.Label;
        if (!string.IsNullOrWhiteSpace(label))
            return label;
        return PascalCaseRegex.Replace(prop.Name, " $1$2");
    }

    private static void DrawMultiReadOnlyValue(string label, object? value, bool mixed)
    {
        if (mixed)
        {
            ImGui.LabelText(label, "(Mixed)");
            return;
        }

        var text = value switch
        {
            null => "(null)",
            bool b => b ? "Yes" : "No",
            float f => $"{f:0.###}",
            double d => $"{d:0.###}",
            Vector2 v2 => $"{v2.X:0.00}, {v2.Y:0.00}",
            Vector3 v3 => $"{v3.X:0.00}, {v3.Y:0.00}, {v3.Z:0.00}",
            Quaternion q => FormatEuler(q),
            _ => value.ToString() ?? "(null)"
        };

        ImGui.LabelText(label, text);
    }

    private static string FormatEuler(Quaternion q)
    {
        var euler = Core.FrinkyMath.QuaternionToEuler(q);
        return $"{euler.X:0.00}, {euler.Y:0.00}, {euler.Z:0.00}";
    }

    private bool DrawMixedSearchableEnum(string label, Type enumType, ref object currentValue, bool mixed)
    {
        if (!enumType.IsEnum)
            return false;

        var mixedLabel = GetMixedLabel(label, mixed);
        var filterId = $"{mixedLabel}_{enumType.FullName}";
        if (!_enumSearchFilters.ContainsKey(filterId))
            _enumSearchFilters[filterId] = "";

        var preview = currentValue?.ToString() ?? "(None)";
        bool changed = false;
        if (ImGui.BeginCombo(mixedLabel, preview))
        {
            var filter = _enumSearchFilters[filterId];
            ImGui.InputTextWithHint("##filter", "Search...", ref filter, 64);
            _enumSearchFilters[filterId] = filter;

            var filterLower = filter.ToLowerInvariant();
            foreach (var value in Enum.GetValues(enumType))
            {
                var name = value.ToString() ?? string.Empty;
                if (filter.Length > 0 && !name.ToLowerInvariant().Contains(filterLower))
                    continue;

                bool isSelected = Equals(value, currentValue);
                if (ImGui.Selectable(name, isSelected))
                {
                    currentValue = value;
                    changed = true;
                }

                if (isSelected)
                    ImGui.SetItemDefaultFocus();
            }

            ImGui.EndCombo();
        }
        else
        {
            _enumSearchFilters[filterId] = "";
        }

        return changed;
    }

    private bool DrawMixedDragFloat(string label, ref float value, bool mixed, float speed, float min = float.MinValue, float max = float.MaxValue)
    {
        bool changed = ImGui.DragFloat(GetMixedLabel(label, mixed), ref value, speed, min, max);
        TrackContinuousUndo();
        return changed;
    }

    private bool DrawMixedDragInt(string label, ref int value, bool mixed, float speed = 1f, int min = int.MinValue, int max = int.MaxValue)
    {
        bool changed = ImGui.DragInt(GetMixedLabel(label, mixed), ref value, speed, min, max);
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

    private static readonly Regex PascalCaseRegex = new(@"(?<=[a-z0-9])([A-Z])|(?<=[A-Z])([A-Z][a-z])", RegexOptions.Compiled);

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

            // Scrollable region so expanded categories don't push content off screen
            float maxHeight = ImGui.GetMainViewport().Size.Y * 0.5f;
            ImGui.BeginChild("##component_list", new Vector2(0, maxHeight), ImGuiChildFlags.None, ImGuiWindowFlags.None);

            if (isSearching)
            {
                // Flat filtered list — search against both display name and type name
                var search = _componentSearch.Trim();
                foreach (var type in allTypes)
                {
                    var displayName = ComponentTypeResolver.GetDisplayName(type);
                    if (!displayName.Contains(search, StringComparison.OrdinalIgnoreCase) &&
                        !type.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                        continue;

                    var source = ComponentTypeResolver.GetAssemblySource(type);
                    if (ImGui.Selectable($"{displayName}  [{source}]"))
                    {
                        AddComponentToEntities(entities, type);
                        ImGui.CloseCurrentPopup();
                    }

                    DrawBaseClassTooltip(type);
                }
            }
            else
            {
                // Grouped by Engine / Game, then by category
                var engineTypes = allTypes.Where(t => ComponentTypeResolver.GetAssemblySource(t) == "Engine").ToList();
                var gameTypes = allTypes.Where(t => ComponentTypeResolver.GetAssemblySource(t) != "Engine").ToList();

                if (engineTypes.Count > 0)
                {
                    if (ImGui.CollapsingHeader("Engine", ImGuiTreeNodeFlags.DefaultOpen))
                    {
                        ImGui.PushID("Engine");
                        DrawCategoryTree(entities, engineTypes);
                        ImGui.PopID();
                    }
                }

                if (gameTypes.Count > 0)
                {
                    if (ImGui.CollapsingHeader("Game", ImGuiTreeNodeFlags.DefaultOpen))
                    {
                        ImGui.PushID("Game");
                        DrawCategoryTree(entities, gameTypes);
                        ImGui.PopID();
                    }
                }
            }

            ImGui.EndChild();

            ImGui.EndPopup();
        }
    }

    private void DrawCategoryTree(IReadOnlyList<Entity> entities, List<Type> types)
    {
        // Build a tree: category path segments -> leaf component types
        var root = new CategoryNode();
        foreach (var type in types)
        {
            var category = ComponentTypeResolver.GetCategory(type);
            var target = root;
            if (!string.IsNullOrEmpty(category))
            {
                var segments = category.Split('/');
                foreach (var segment in segments)
                {
                    if (!target.Children.TryGetValue(segment, out var child))
                    {
                        child = new CategoryNode();
                        target.Children[segment] = child;
                    }
                    target = child;
                }
            }
            target.Types.Add(type);
        }

        DrawCategoryNode(entities, root);
    }

    private void DrawCategoryNode(IReadOnlyList<Entity> entities, CategoryNode node)
    {
        // Draw uncategorized types first (directly under this node)
        foreach (var type in node.Types)
        {
            var displayName = ComponentTypeResolver.GetDisplayName(type);
            if (ImGui.Selectable($"  {displayName}"))
            {
                AddComponentToEntities(entities, type);
                ImGui.CloseCurrentPopup();
            }
            DrawBaseClassTooltip(type);
        }

        // Draw child categories as tree nodes
        foreach (var (categoryName, child) in node.Children.OrderBy(kv => kv.Key))
        {
            if (ImGui.TreeNode(categoryName))
            {
                DrawCategoryNode(entities, child);
                ImGui.TreePop();
            }
        }
    }

    private class CategoryNode
    {
        public Dictionary<string, CategoryNode> Children { get; } = new();
        public List<Type> Types { get; } = new();
    }

    private void AddComponentToEntities(IReadOnlyList<Entity> entities, Type type)
    {
        _app.RecordUndo();
        bool anyAdded = false;
        foreach (var entity in entities)
        {
            if (entity.GetComponent(type) == null)
            {
                if (entity.TryAddComponent(type, out _, out var failureReason))
                {
                    anyAdded = true;
                }
                else
                {
                    NotificationManager.Instance.Post(
                        failureReason ?? $"Failed to add {ComponentTypeResolver.GetDisplayName(type)} to {entity.Name}.",
                        NotificationType.Warning);
                }
            }
        }

        if (anyAdded)
            _app.RefreshUndoBaseline();
    }

    private static void DrawBaseClassTooltip(Type type)
    {
        if (ImGui.IsItemHovered() && type.BaseType != null && type.BaseType != typeof(Component))
        {
            ImGui.SetTooltip($"Extends {type.BaseType.Name}");
        }
    }

    private static unsafe void DrawBoneHierarchyTree(SkinnedMeshAnimatorComponent animator, Entity entity)
    {
        var meshRenderer = entity.GetComponent<MeshRendererComponent>();
        if (meshRenderer == null || meshRenderer.ModelPath.IsEmpty)
            return;

        var model = AssetManager.Instance.LoadModel(meshRenderer.ModelPath.Path);
        if (model.BoneCount <= 0)
            return;

        ImGui.Separator();
        if (!ImGui.CollapsingHeader($"Bone Hierarchy ({model.BoneCount} bones)"))
            return;

        // Build children lookup
        var children = new Dictionary<int, List<int>>();
        var roots = new List<int>();

        for (int i = 0; i < model.BoneCount; i++)
        {
            int parent = model.Bones[i].Parent;
            if (parent < 0 || parent >= model.BoneCount)
            {
                roots.Add(i);
            }
            else
            {
                if (!children.TryGetValue(parent, out var list))
                {
                    list = new List<int>();
                    children[parent] = list;
                }
                list.Add(i);
            }
        }

        foreach (int root in roots)
            DrawBoneNode(model, root, children);
    }

    private static unsafe void DrawBoneNode(Model model, int boneIndex, Dictionary<int, List<int>> children)
    {
        var boneName = new string(model.Bones[boneIndex].Name, 0, 32).TrimEnd('\0');
        if (string.IsNullOrWhiteSpace(boneName))
            boneName = $"Bone {boneIndex}";

        bool hasChildren = children.ContainsKey(boneIndex);
        var flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanAvailWidth;
        if (!hasChildren)
            flags |= ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen;

        bool opened = ImGui.TreeNodeEx($"{boneName}##{boneIndex}", flags);

        if (hasChildren && opened)
        {
            foreach (int child in children[boneIndex])
                DrawBoneNode(model, child, children);
            ImGui.TreePop();
        }
    }
}
