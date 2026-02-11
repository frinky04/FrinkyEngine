using System.Globalization;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using FrinkyEngine.Core.Audio;
using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.Components;
using FrinkyEngine.Core.ECS;
using FrinkyEngine.Core.Rendering;
using FrinkyEngine.Core.Scene;
using FrinkyEngine.Core.Serialization;
using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Widgets;
using Raylib_cs;
using Texture2D = Raylib_cs.Texture2D;

namespace FrinkyEngine.Editor.Panels;

public static class ComponentDrawerRegistry
{
    private static readonly Dictionary<Type, Action<Component>> _drawers = new();

    static ComponentDrawerRegistry()
    {
        Register<TransformComponent>(DrawTransform);
        Register<MeshRendererComponent>(DrawMeshRenderer);
        Register<PrimitiveComponent>(DrawPrimitive);
    }

    public static void Register<T>(Action<Component> drawer) where T : Component
    {
        _drawers[typeof(T)] = drawer;
    }

    public static bool Draw(Component component)
    {
        var type = component.GetType();
        while (type != null && type != typeof(Component))
        {
            if (_drawers.TryGetValue(type, out var drawer))
            {
                drawer(component);
                return true;
            }
            type = type.BaseType;
        }
        return false;
    }

    private static readonly Vector4 ColorRed = new(0.867f, 0.2f, 0.267f, 1f);    // #DD3344
    private static readonly Vector4 ColorRedHover = new(0.933f, 0.3f, 0.367f, 1f);
    private static readonly Vector4 ColorGreen = new(0.267f, 0.733f, 0.267f, 1f);  // #44BB44
    private static readonly Vector4 ColorGreenHover = new(0.367f, 0.833f, 0.367f, 1f);
    private static readonly Vector4 ColorBlue = new(0.267f, 0.533f, 0.867f, 1f);   // #4488DD
    private static readonly Vector4 ColorBlueHover = new(0.367f, 0.633f, 0.933f, 1f);

    private static void DrawTransform(Component c)
    {
        var t = (TransformComponent)c;
        var pos = t.LocalPosition;
        if (DrawColoredVector3("Position", ref pos, 0.1f))
            t.LocalPosition = pos;

        var euler = t.EulerAngles;
        if (DrawColoredVector3("Rotation", ref euler, 0.5f))
            t.EulerAngles = euler;

        var scale = t.LocalScale;
        if (DrawColoredVector3("Scale", ref scale, 0.05f, 1f))
            t.LocalScale = scale;
    }

    private static bool DrawColoredVector3(string label, ref Vector3 value, float speed, float resetValue = 0f)
    {
        bool changed = false;
        var app = EditorApplication.Instance;
        ImGui.PushID(label);

        ImGui.Columns(2, (string?)null, false);
        ImGui.SetColumnWidth(0, 80);
        ImGui.Text(label);
        ImGui.NextColumn();

        float lineHeight = ImGui.GetFrameHeight();
        var buttonSize = new Vector2(lineHeight + 3f, lineHeight);
        float availWidth = ImGui.GetContentRegionAvail().X;
        float spacing = ImGui.GetStyle().ItemSpacing.X;
        float fieldWidth = (availWidth - 3f * buttonSize.X - 5f * spacing) / 3f;

        // X
        ImGui.PushStyleColor(ImGuiCol.Button, ColorRed);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ColorRedHover);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, ColorRed);
        if (ImGui.Button("X", buttonSize))
        {
            app.RecordUndo();
            value.X = resetValue;
            changed = true;
            app.RefreshUndoBaseline();
        }
        ImGui.PopStyleColor(3);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(fieldWidth);
        if (ImGui.DragFloat("##X", ref value.X, speed))
            changed = true;
        TrackContinuousUndo(app);

        ImGui.SameLine();

        // Y
        ImGui.PushStyleColor(ImGuiCol.Button, ColorGreen);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ColorGreenHover);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, ColorGreen);
        if (ImGui.Button("Y", buttonSize))
        {
            app.RecordUndo();
            value.Y = resetValue;
            changed = true;
            app.RefreshUndoBaseline();
        }
        ImGui.PopStyleColor(3);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(fieldWidth);
        if (ImGui.DragFloat("##Y", ref value.Y, speed))
            changed = true;
        TrackContinuousUndo(app);

        ImGui.SameLine();

        // Z
        ImGui.PushStyleColor(ImGuiCol.Button, ColorBlue);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ColorBlueHover);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, ColorBlue);
        if (ImGui.Button("Z", buttonSize))
        {
            app.RecordUndo();
            value.Z = resetValue;
            changed = true;
            app.RefreshUndoBaseline();
        }
        ImGui.PopStyleColor(3);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(fieldWidth);
        if (ImGui.DragFloat("##Z", ref value.Z, speed))
            changed = true;
        TrackContinuousUndo(app);

        ImGui.Columns(1);
        ImGui.PopID();
        return changed;
    }

    private static void DrawMeshRenderer(Component c)
    {
        var mr = (MeshRendererComponent)c;
        var app = EditorApplication.Instance;

        DrawAssetReference("Model Path", mr.ModelPath, AssetType.Model, v => mr.ModelPath = v);

        ImGui.Spacing();

        var tint = ColorToVec4(mr.Tint);
        if (ImGui.ColorEdit4("Tint", ref tint))
            mr.Tint = Vec4ToColor(tint);
        TrackContinuousUndo(app);

        // Material Slots
        if (mr.MaterialSlots.Count > 0)
        {
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Text("Material Slots");
            ImGui.Spacing();

            bool slotsChanged = false;
            for (int i = 0; i < mr.MaterialSlots.Count; i++)
            {
                var slot = mr.MaterialSlots[i];
                ImGui.PushID(i);

                if (ImGui.TreeNode($"Slot {i}"))
                {
                    var matType = slot.MaterialType;
                    if (ComboEnumHelper<MaterialType>.Combo("Type", ref matType))
                    {
                        app.RecordUndo();
                        slot.MaterialType = matType;
                        slotsChanged = true;
                        app.RefreshUndoBaseline();
                    }

                    if (slot.MaterialType is MaterialType.Textured or MaterialType.TriplanarTexture)
                    {
                        DrawAssetReference("Texture", slot.TexturePath, AssetType.Texture, v =>
                        {
                            slot.TexturePath = v;
                            slotsChanged = true;
                        });
                    }

                    if (slot.MaterialType == MaterialType.TriplanarTexture)
                    {
                        float scale = slot.TriplanarScale;
                        if (ImGui.DragFloat("Triplanar Scale", ref scale, 0.05f, 0.01f, 512f))
                        {
                            slot.TriplanarScale = scale;
                            slotsChanged = true;
                        }
                        TrackContinuousUndo(app);

                        float sharpness = slot.TriplanarBlendSharpness;
                        if (ImGui.DragFloat("Blend Sharpness", ref sharpness, 0.05f, 0.01f, 64f))
                        {
                            slot.TriplanarBlendSharpness = sharpness;
                            slotsChanged = true;
                        }
                        TrackContinuousUndo(app);

                        bool useWorldSpace = slot.TriplanarUseWorldSpace;
                        if (ImGui.Checkbox("Use World Space", ref useWorldSpace))
                        {
                            app.RecordUndo();
                            slot.TriplanarUseWorldSpace = useWorldSpace;
                            slotsChanged = true;
                            app.RefreshUndoBaseline();
                        }
                    }

                    ImGui.TreePop();
                }

                ImGui.PopID();
            }

            if (slotsChanged)
                mr.RefreshMaterials();
        }
    }

    private static void DrawPrimitive(Component c)
    {
        var prim = (PrimitiveComponent)c;
        var app = EditorApplication.Instance;

        var matType = prim.MaterialType;
        if (ComboEnumHelper<MaterialType>.Combo("Material Type", ref matType))
        {
            app.RecordUndo();
            prim.MaterialType = matType;
            app.RefreshUndoBaseline();
        }

        var tint = ColorToVec4(prim.Tint);
        if (ImGui.ColorEdit4("Tint", ref tint))
            prim.Tint = Vec4ToColor(tint);
        TrackContinuousUndo(app);

        if (prim.MaterialType is MaterialType.Textured or MaterialType.TriplanarTexture)
        {
            DrawAssetReference("Texture Path", prim.TexturePath, AssetType.Texture, v => prim.TexturePath = v);
        }

        if (prim.MaterialType == MaterialType.TriplanarTexture)
        {
            float scale = prim.TriplanarScale;
            if (ImGui.DragFloat("Triplanar Scale", ref scale, 0.05f, 0.01f, 512f))
                prim.TriplanarScale = scale;
            TrackContinuousUndo(app);

            float sharpness = prim.TriplanarBlendSharpness;
            if (ImGui.DragFloat("Blend Sharpness", ref sharpness, 0.05f, 0.01f, 64f))
                prim.TriplanarBlendSharpness = sharpness;
            TrackContinuousUndo(app);

            bool useWorldSpace = prim.TriplanarUseWorldSpace;
            if (ImGui.Checkbox("Use World Space", ref useWorldSpace))
            {
                app.RecordUndo();
                prim.TriplanarUseWorldSpace = useWorldSpace;
                app.RefreshUndoBaseline();
            }
        }

        ImGui.Separator();
        ImGui.Text("Shape Properties");
        ImGui.Spacing();

        DrawSubclassProperties(c);
    }

    private static void DrawSubclassProperties(Component component)
    {
        string? lastSection = null;
        string? lastHeader = null;
        var type = component.GetType();
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            if (!prop.CanRead || !prop.CanWrite) continue;
            ApplyLayoutAttributes(prop, ref lastSection, ref lastHeader);
            DrawProperty(component, prop);
        }
    }

    public static void DrawReflection(Component component)
    {
        string? lastSection = null;
        string? lastHeader = null;
        var type = component.GetType();
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!IsInspectableProperty(prop))
                continue;
            if (!IsPropertyVisible(component, prop))
                continue;

            ApplyLayoutAttributes(prop, ref lastSection, ref lastHeader);
            DrawProperty(component, prop);
        }

        DrawReflectionExtensions(component);
    }

    private static void ApplyLayoutAttributes(PropertyInfo prop, ref string? lastSection, ref string? lastHeader)
    {
        // Space
        var spaceAttr = prop.GetCustomAttribute<InspectorSpaceAttribute>();
        if (spaceAttr != null)
            ImGui.Dummy(new Vector2(0, spaceAttr.Height));

        // Section (separator text)
        var section = prop.GetCustomAttribute<InspectorSectionAttribute>()?.Title;
        if (!string.IsNullOrWhiteSpace(section) && !string.Equals(section, lastSection, StringComparison.Ordinal))
        {
            DrawSection(section!);
            lastSection = section;
        }

        // Header (collapsing header style, but just text for now)
        var headerAttr = prop.GetCustomAttribute<InspectorHeaderAttribute>();
        if (headerAttr != null && !string.Equals(headerAttr.Title, lastHeader, StringComparison.Ordinal))
        {
            ImGui.TextDisabled(headerAttr.Title);
            lastHeader = headerAttr.Title;
        }

        // Indent
        var indentAttr = prop.GetCustomAttribute<InspectorIndentAttribute>();
        if (indentAttr != null)
            ImGui.Indent(indentAttr.Levels * 16f);
    }

    private static void DrawPropertyWithTooltip(PropertyInfo prop, Action drawAction)
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

    private static void DrawProperty(Component component, PropertyInfo prop)
    {
        var propType = prop.PropertyType;
        var label = GetInspectorLabel(prop);
        var app = EditorApplication.Instance;
        var isReadOnly = !prop.CanWrite || prop.GetCustomAttribute<InspectorReadOnlyAttribute>() != null;

        if (isReadOnly)
        {
            DrawReadOnlyValue(component, prop, label);
            EndLayoutAttributes(prop);
            return;
        }

        var rangeAttr = prop.GetCustomAttribute<InspectorRangeAttribute>();

        if (propType == typeof(float))
        {
            float val = (float)prop.GetValue(component)!;
            float speed = rangeAttr?.Speed ?? 0.1f;
            float min = rangeAttr?.Min ?? float.MinValue;
            float max = rangeAttr?.Max ?? float.MaxValue;
            DrawPropertyWithTooltip(prop, () =>
            {
                if (ImGui.DragFloat(label, ref val, speed, min, max))
                    prop.SetValue(component, val);
                TrackContinuousUndo(app);
            });
        }
        else if (propType == typeof(int))
        {
            int val = (int)prop.GetValue(component)!;
            float speed = rangeAttr?.Speed ?? 1f;
            int min = (int)(rangeAttr?.Min ?? int.MinValue);
            int max = (int)(rangeAttr?.Max ?? int.MaxValue);
            DrawPropertyWithTooltip(prop, () =>
            {
                if (ImGui.DragInt(label, ref val, speed, min, max))
                    prop.SetValue(component, val);
                TrackContinuousUndo(app);
            });
        }
        else if (propType == typeof(bool))
        {
            DrawPropertyWithTooltip(prop, () =>
                DrawCheckbox(label, (bool)prop.GetValue(component)!, v => prop.SetValue(component, v)));
        }
        else if (propType == typeof(string))
        {
            string val = (string)(prop.GetValue(component) ?? "");
            DrawPropertyWithTooltip(prop, () =>
            {
                if (ImGui.InputText(label, ref val, 256))
                    prop.SetValue(component, val);
                TrackContinuousUndo(app);
            });
        }
        else if (propType == typeof(Vector3))
        {
            var val = (Vector3)prop.GetValue(component)!;
            DrawPropertyWithTooltip(prop, () =>
            {
                if (DrawColoredVector3(label, ref val, 0.1f))
                    prop.SetValue(component, val);
            });
        }
        else if (propType == typeof(Vector2))
        {
            var val = (Vector2)prop.GetValue(component)!;
            DrawPropertyWithTooltip(prop, () =>
            {
                if (ImGui.DragFloat2(label, ref val, 0.1f))
                    prop.SetValue(component, val);
                TrackContinuousUndo(app);
            });
        }
        else if (propType == typeof(Quaternion))
        {
            var q = (Quaternion)prop.GetValue(component)!;
            var euler = Core.FrinkyMath.QuaternionToEuler(q);
            DrawPropertyWithTooltip(prop, () =>
            {
                if (DrawColoredVector3(label, ref euler, 0.5f))
                    prop.SetValue(component, Core.FrinkyMath.EulerToQuaternion(euler));
            });
        }
        else if (propType == typeof(Color))
        {
            DrawPropertyWithTooltip(prop, () =>
                DrawColorEdit4(label, (Color)prop.GetValue(component)!, v => prop.SetValue(component, v)));
        }
        else if (propType.IsEnum)
        {
            object currentValue = prop.GetValue(component) ?? Enum.GetValues(propType).GetValue(0)!;
            DrawPropertyWithTooltip(prop, () =>
            {
                bool changed = prop.GetCustomAttribute<InspectorSearchableEnumAttribute>() != null
                    ? DrawSearchableEnumCombo(label, propType, ref currentValue)
                    : ComboEnumHelper.Combo(label, propType, ref currentValue);
                if (changed)
                {
                    app.RecordUndo();
                    prop.SetValue(component, currentValue);
                    app.RefreshUndoBaseline();
                }
            });
        }
        else if (propType == typeof(EntityReference))
        {
            DrawEntityReference(label, component, prop);
        }
        else if (propType == typeof(AssetReference))
        {
            var assetRef = (AssetReference)prop.GetValue(component)!;
            var filterAttr = prop.GetCustomAttribute<AssetFilterAttribute>();
            var assetFilter = filterAttr?.Filter ?? AssetType.Unknown;
            DrawAssetReference(label, assetRef, assetFilter, v =>
            {
                prop.SetValue(component, v);
            });
        }
        else if (propType == typeof(AudioAttenuationSettings))
        {
            if (component is AudioSourceComponent source)
                DrawAudioSourceAttenuation(source, prop);
            else
                ImGui.LabelText(label, propType.Name);
        }
        else if (typeof(FObject).IsAssignableFrom(propType))
        {
            DrawFObjectProperty(label, component, prop, propType);
        }
        else if (IsFObjectListType(propType, out var fobjectElementType))
        {
            DrawFObjectListProperty(label, component, prop, fobjectElementType!);
        }
        else
        {
            ImGui.LabelText(label, propType.Name);
        }

        EndLayoutAttributes(prop);
    }

    private static void EndLayoutAttributes(PropertyInfo prop)
    {
        var indentAttr = prop.GetCustomAttribute<InspectorIndentAttribute>();
        if (indentAttr != null)
            ImGui.Unindent(indentAttr.Levels * 16f);
    }

    private static void DrawAudioSourceAttenuation(AudioSourceComponent source, PropertyInfo prop)
    {
        var app = EditorApplication.Instance;
        var attenuation = source.Attenuation;
        bool changed = false;

        if (source.Spatialized)
        {
            float minDistance = attenuation.MinDistance;
            if (ImGui.DragFloat("Min Distance", ref minDistance, 0.1f, 0f, 10000f))
            {
                attenuation.MinDistance = minDistance;
                changed = true;
            }
            TrackContinuousUndo(app);

            float maxDistance = attenuation.MaxDistance;
            if (ImGui.DragFloat("Max Distance", ref maxDistance, 0.1f, 0f, 10000f))
            {
                attenuation.MaxDistance = maxDistance;
                changed = true;
            }
            TrackContinuousUndo(app);

            var rolloff = attenuation.Rolloff;
            if (ComboEnumHelper<AudioRolloffMode>.Combo("Rolloff", ref rolloff))
            {
                app.RecordUndo();
                attenuation.Rolloff = rolloff;
                changed = true;
                app.RefreshUndoBaseline();
            }

            float spatialBlend = attenuation.SpatialBlend;
            if (ImGui.DragFloat("Spatial Blend", ref spatialBlend, 0.01f, 0f, 1f))
            {
                attenuation.SpatialBlend = spatialBlend;
                changed = true;
            }
            TrackContinuousUndo(app);
        }
        else
        {
            float pan = attenuation.PanStereo;
            if (ImGui.DragFloat("Stereo Pan", ref pan, 0.01f, -1f, 1f))
            {
                attenuation.PanStereo = pan;
                changed = true;
            }
            TrackContinuousUndo(app);
        }

        if (changed)
            prop.SetValue(source, attenuation);
    }

    private static bool IsInspectableProperty(PropertyInfo prop)
    {
        if (!prop.CanRead)
            return false;
        if (prop.Name is "Entity" or "HasStarted" or "Enabled" or "RenderModel")
            return false;
        if (prop.CanWrite)
            return true;
        return prop.GetCustomAttribute<InspectorReadOnlyAttribute>() != null;
    }

    private static bool IsPropertyVisible(Component component, PropertyInfo prop)
    {
        foreach (var visibleIf in prop.GetCustomAttributes<InspectorVisibleIfAttribute>())
        {
            var condition = component.GetType().GetProperty(visibleIf.PropertyName, BindingFlags.Public | BindingFlags.Instance);
            if (condition == null || !condition.CanRead || condition.PropertyType != typeof(bool))
                return false;

            var current = (bool?)condition.GetValue(component) ?? false;
            if (current != visibleIf.ExpectedValue)
                return false;
        }

        foreach (var visibleIfEnum in prop.GetCustomAttributes<InspectorVisibleIfEnumAttribute>())
        {
            var condition = component.GetType().GetProperty(visibleIfEnum.PropertyName, BindingFlags.Public | BindingFlags.Instance);
            if (condition == null || !condition.CanRead || !condition.PropertyType.IsEnum)
                return false;

            var value = condition.GetValue(component);
            if (value is not Enum enumValue)
                return false;
            if (!string.Equals(enumValue.ToString(), visibleIfEnum.ExpectedMemberName, StringComparison.Ordinal))
                return false;
        }

        return true;
    }

    private static string GetInspectorLabel(PropertyInfo prop)
    {
        var attrLabel = prop.GetCustomAttribute<InspectorLabelAttribute>()?.Label;
        if (!string.IsNullOrWhiteSpace(attrLabel))
            return attrLabel;
        return NiceLabel(prop.Name);
    }

    private static void DrawReadOnlyValue(Component component, PropertyInfo prop, string label)
    {
        var value = prop.GetValue(component);
        switch (value)
        {
            case null:
                DrawReadOnlyText(label, "(null)");
                break;
            case bool b:
                DrawReadOnlyText(label, b ? "Yes" : "No");
                break;
            case float f:
                DrawReadOnlyText(label, $"{f:0.###}");
                break;
            case double d:
                DrawReadOnlyText(label, $"{d:0.###}");
                break;
            case Vector2 v2:
                DrawReadOnlyText(label, $"{v2.X:0.00}, {v2.Y:0.00}");
                break;
            case Vector3 v3:
                DrawReadOnlyText(label, $"{v3.X:0.00}, {v3.Y:0.00}, {v3.Z:0.00}");
                break;
            case Quaternion q:
                var euler = Core.FrinkyMath.QuaternionToEuler(q);
                DrawReadOnlyText(label, $"{euler.X:0.00}, {euler.Y:0.00}, {euler.Z:0.00}");
                break;
            default:
                DrawReadOnlyText(label, value.ToString() ?? "(null)");
                break;
        }
    }

    private static void DrawReflectionExtensions(Component component)
    {
        DrawReflectionWarnings(component);
        if (component is AudioSourceComponent source)
            DrawAudioSourceRuntimeControls(source);
    }

    private static void DrawAudioSourceRuntimeControls(AudioSourceComponent source)
    {
        var app = EditorApplication.Instance;
        ImGui.Separator();

        if (app.IsInRuntimeMode)
        {
            if (ImGui.Button("Play##AudioSource"))
                source.Play();

            ImGui.SameLine();
            if (ImGui.Button("Pause##AudioSource"))
                source.Pause();

            ImGui.SameLine();
            if (ImGui.Button("Resume##AudioSource"))
                source.Resume();

            ImGui.SameLine();
            if (ImGui.Button("Stop##AudioSource"))
                source.Stop();
        }
        else
        {
            ImGui.TextDisabled("Runtime controls available in Play/Simulate.");
        }
    }

    private static void DrawReflectionWarnings(Component component)
    {
        switch (component)
        {
            case RigidbodyComponent rb:
                bool hasCollider = rb.Entity.Components.Any(c => c is ColliderComponent collider && collider.Enabled);
                if (!hasCollider)
                {
                    ImGui.Spacing();
                    DrawWarning("Rigidbody is ignored until an enabled collider component is added.");
                }

                if (rb.Entity.Transform.Parent != null)
                    DrawWarning("Parented rigidbodies are not simulated.");
                break;

            case CharacterControllerComponent controller:
                var entity = controller.Entity;
                var rigidbody = entity.GetComponent<RigidbodyComponent>();
                var capsule = entity.GetComponent<CapsuleColliderComponent>();

                if (rigidbody == null || !rigidbody.Enabled)
                    DrawWarning("Character controller requires an enabled RigidbodyComponent.");
                else if (rigidbody.MotionType != BodyMotionType.Dynamic)
                    DrawWarning("Character controller requires Rigidbody Motion Type = Dynamic.");

                if (capsule == null || !capsule.Enabled)
                {
                    DrawWarning("Character controller requires an enabled CapsuleColliderComponent.");
                }
                else
                {
                    var primaryCollider = entity.Components
                        .OfType<ColliderComponent>()
                        .FirstOrDefault(col => col.Enabled);
                    if (!ReferenceEquals(primaryCollider, capsule))
                        DrawWarning("The capsule must be the first enabled collider on the entity.");
                }

                if (entity.Transform.Parent != null)
                    DrawWarning("Parented character controllers are not simulated.");
                break;

            case ColliderComponent collider:
                int colliderCount = collider.Entity.Components.Count(c => c is ColliderComponent enabledCollider && enabledCollider.Enabled);
                if (colliderCount > 1)
                    DrawWarning("Multiple enabled colliders are present. Only the first enabled collider is used.");
                break;
        }
    }

    private static readonly Dictionary<string, string> _entityRefFilters = new();

    private static unsafe void DrawEntityReference(string label, Component component, PropertyInfo prop)
    {
        var app = EditorApplication.Instance;
        var scene = app.CurrentScene;
        var entityRef = (EntityReference)(prop.GetValue(component) ?? EntityReference.None);

        Entity? resolved = null;
        string preview;
        if (!entityRef.IsValid)
        {
            preview = "(None)";
        }
        else
        {
            resolved = scene?.FindEntityById(entityRef.Id);
            preview = resolved != null ? resolved.Name : "(Missing)";
        }

        var filterId = label;

        ImGui.PushID(label);
        ImGui.Columns(2, (string?)null, false);
        ImGui.SetColumnWidth(0, 80);
        ImGui.Text(label);
        ImGui.NextColumn();

        float availWidth = ImGui.GetContentRegionAvail().X;
        float clearButtonWidth = ImGui.CalcTextSize("X").X + ImGui.GetStyle().FramePadding.X * 2f;
        float comboWidth = availWidth - clearButtonWidth - ImGui.GetStyle().ItemSpacing.X;

        ImGui.SetNextItemWidth(comboWidth);
        if (ImGui.BeginCombo("##ref", preview))
        {
            if (!_entityRefFilters.ContainsKey(filterId))
                _entityRefFilters[filterId] = "";
            var filter = _entityRefFilters[filterId];
            ImGui.InputTextWithHint("##entityFilter", "Search...", ref filter, 64);
            _entityRefFilters[filterId] = filter;

            if (ImGui.Selectable("(None)", !entityRef.IsValid))
            {
                app.RecordUndo();
                prop.SetValue(component, EntityReference.None);
                app.RefreshUndoBaseline();
            }

            if (scene != null)
            {
                foreach (var entity in scene.Entities)
                {
                    if (filter.Length > 0 && !entity.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
                        continue;

                    bool isSelected = entityRef.IsValid && entity.Id == entityRef.Id;
                    if (ImGui.Selectable(entity.Name + "##" + entity.Id.ToString("N"), isSelected))
                    {
                        app.RecordUndo();
                        prop.SetValue(component, new EntityReference(entity));
                        app.RefreshUndoBaseline();
                    }
                }
            }

            ImGui.EndCombo();
        }
        else
        {
            _entityRefFilters[filterId] = "";
        }

        // Drag-and-drop target on the combo
        if (ImGui.BeginDragDropTarget())
        {
            ImGuiPayload* payload = ImGui.AcceptDragDropPayload("FRINKY_HIERARCHY_ENTITY");
            if (payload != null && payload->Delivery != 0 && app.DraggedEntityId.HasValue)
            {
                var draggedEntity = app.FindEntityById(app.DraggedEntityId.Value);
                if (draggedEntity != null)
                {
                    app.RecordUndo();
                    prop.SetValue(component, new EntityReference(draggedEntity));
                    app.RefreshUndoBaseline();
                }
            }
            ImGui.EndDragDropTarget();
        }

        ImGui.SameLine();
        if (ImGui.Button("X") && entityRef.IsValid)
        {
            app.RecordUndo();
            prop.SetValue(component, EntityReference.None);
            app.RefreshUndoBaseline();
        }

        ImGui.Columns(1);
        ImGui.PopID();
    }

    private static readonly Dictionary<string, string> _assetRefFilters = new();

    private static unsafe void DrawAssetReference(string label, AssetReference current, AssetType filter, Action<AssetReference> setter)
    {
        var app = EditorApplication.Instance;
        var db = AssetDatabase.Instance;

        bool isEngineRef = current.IsEngineAsset;
        string preview = current.IsEmpty
            ? "(None)"
            : (isEngineRef ? "[E] " : "") + Path.GetFileName(isEngineRef ? AssetReference.StripEnginePrefix(current.Path) : current.Path);
        bool isBroken = !current.IsEmpty && !db.AssetExistsByName(current.Path);

        var filterId = $"AssetRef_{label}";

        ImGui.PushID(label);
        ImGui.Text(label);

        float availWidth = ImGui.GetContentRegionAvail().X;
        float clearButtonWidth = ImGui.CalcTextSize("X").X + ImGui.GetStyle().FramePadding.X * 2f;
        float comboWidth = availWidth - clearButtonWidth - ImGui.GetStyle().ItemSpacing.X;

        if (isBroken)
            ImGui.PushStyleColor(ImGuiCol.Text, WarningColor);

        ImGui.SetNextItemWidth(comboWidth);
        if (ImGui.BeginCombo("##assetref", preview))
        {
            if (isBroken)
                ImGui.PopStyleColor();

            if (!_assetRefFilters.ContainsKey(filterId))
                _assetRefFilters[filterId] = "";
            var searchFilter = _assetRefFilters[filterId];
            ImGui.InputTextWithHint("##assetFilter", "Search...", ref searchFilter, 128);
            _assetRefFilters[filterId] = searchFilter;

            // (None) option
            if (ImGui.Selectable("(None)", current.IsEmpty))
            {
                app.RecordUndo();
                setter(new AssetReference(""));
                app.RefreshUndoBaseline();
            }

            var tagDb = app.TagDatabase;
            var assets = filter == AssetType.Unknown ? db.GetAssets(null) : db.GetAssets(filter);
            foreach (var asset in assets)
            {
                var tags = tagDb?.GetTagsForAsset(asset.RelativePath);

                bool matchesSearch = true;
                if (searchFilter.Length > 0)
                {
                    matchesSearch = asset.RelativePath.Contains(searchFilter, StringComparison.OrdinalIgnoreCase);
                    if (!matchesSearch && tags != null)
                        matchesSearch = tags.Any(t => t.Name.Contains(searchFilter, StringComparison.OrdinalIgnoreCase));
                }

                if (!matchesSearch)
                    continue;

                ImGui.PushID(asset.RelativePath);

                // Draw asset icon + label (show filename; tooltip shows full path if ambiguous)
                var displayName = db.IsFileNameUnique(asset.FileName)
                    ? asset.FileName
                    : asset.RelativePath;
                if (AssetSelectable(asset.Type, displayName))
                {
                    app.RecordUndo();
                    setter(new AssetReference(db.GetCanonicalName(asset.RelativePath)));
                    app.RefreshUndoBaseline();
                }
                if (!db.IsFileNameUnique(asset.FileName) && ImGui.IsItemHovered())
                    ImGui.SetTooltip(asset.RelativePath);

                // Draw tag chips on the same line
                if (tags is { Count: > 0 })
                {
                    ImGui.SameLine();
                    foreach (var tag in tags)
                    {
                        ImGui.SameLine();
                        ImGui.TextColored(ParseHexColor(tag.Color), $"[{tag.Name}]");
                    }
                }

                ImGui.PopID();
            }

            // Engine assets
            ImGui.Separator();
            ImGui.TextDisabled("--- Engine Content ---");
            var engineAssets = filter == AssetType.Unknown ? db.GetEngineAssets(null) : db.GetEngineAssets(filter);
            foreach (var engineAsset in engineAssets)
            {
                bool matchesSearch = true;
                if (searchFilter.Length > 0)
                    matchesSearch = engineAsset.RelativePath.Contains(searchFilter, StringComparison.OrdinalIgnoreCase);

                if (!matchesSearch)
                    continue;

                ImGui.PushID("engine_" + engineAsset.RelativePath);

                var displayName = db.IsEngineFileNameUnique(engineAsset.FileName)
                    ? "[E] " + engineAsset.FileName
                    : "[E] " + engineAsset.RelativePath;
                if (AssetSelectable(engineAsset.Type, displayName))
                {
                    app.RecordUndo();
                    setter(new AssetReference(db.GetEngineCanonicalName(engineAsset.RelativePath)));
                    app.RefreshUndoBaseline();
                }
                if (!db.IsEngineFileNameUnique(engineAsset.FileName) && ImGui.IsItemHovered())
                    ImGui.SetTooltip(AssetReference.EnginePrefix + engineAsset.RelativePath);

                ImGui.PopID();
            }

            ImGui.EndCombo();
        }
        else
        {
            if (isBroken)
                ImGui.PopStyleColor();
            if (!current.IsEmpty && ImGui.IsItemHovered())
            {
                var resolvedPath = db.ResolveAssetPath(current.Path) ?? current.Path;
                ImGui.SetTooltip(resolvedPath);
            }
            _assetRefFilters[filterId] = "";
        }

        // Drag-and-drop target
        if (ImGui.BeginDragDropTarget())
        {
            ImGuiPayload* payload = ImGui.AcceptDragDropPayload(AssetBrowserPanel.AssetDragPayload);
            if (payload != null && payload->Delivery != 0)
            {
                var draggedPath = app.DraggedAssetPath;
                if (!string.IsNullOrEmpty(draggedPath))
                {
                    if (AssetReference.HasEnginePrefix(draggedPath))
                    {
                        var stripped = AssetReference.StripEnginePrefix(draggedPath);
                        var draggedAsset = db.GetEngineAssets()
                            .FirstOrDefault(a => string.Equals(a.RelativePath, stripped, StringComparison.OrdinalIgnoreCase));

                        if (draggedAsset != null && (filter == AssetType.Unknown || draggedAsset.Type == filter))
                        {
                            app.RecordUndo();
                            setter(new AssetReference(db.GetEngineCanonicalName(stripped)));
                            app.RefreshUndoBaseline();
                        }
                    }
                    else
                    {
                        var draggedAsset = db.GetAssets()
                            .FirstOrDefault(a => string.Equals(a.RelativePath, draggedPath, StringComparison.OrdinalIgnoreCase));

                        if (draggedAsset != null && (filter == AssetType.Unknown || draggedAsset.Type == filter))
                        {
                            app.RecordUndo();
                            setter(new AssetReference(db.GetCanonicalName(draggedPath)));
                            app.RefreshUndoBaseline();
                        }
                    }
                }
            }
            ImGui.EndDragDropTarget();
        }

        // Clear button
        ImGui.SameLine();
        if (ImGui.Button("X") && !current.IsEmpty)
        {
            app.RecordUndo();
            setter(new AssetReference(""));
            app.RefreshUndoBaseline();
        }

        ImGui.PopID();
    }

    private static bool AssetSelectable(AssetType type, string label)
    {
        float iconSize = DrawAssetIcon(type);

        var textPos = ImGui.GetCursorPos();
        bool clicked = ImGui.Selectable("##sel", false, ImGuiSelectableFlags.None, new Vector2(0, iconSize));

        var afterPos = ImGui.GetCursorPos();
        float textH = ImGui.GetTextLineHeight();
        ImGui.SetCursorPos(new Vector2(textPos.X, textPos.Y + Math.Max(0f, (iconSize - textH) * 0.5f)));
        ImGui.TextUnformatted(label);
        ImGui.SetCursorPos(afterPos);
        ImGui.Dummy(Vector2.Zero);

        return clicked;
    }

    private static unsafe float DrawAssetIcon(AssetType type)
    {
        float size = EditorIcons.GetIconSize();
        var icon = EditorIcons.GetIcon(type);
        if (icon is Texture2D tex)
        {
            ImGui.Image(new ImTextureRef(null, new ImTextureID((ulong)tex.Id)), new Vector2(size, size));
            ImGui.SameLine(0, 4);
        }
        return size;
    }

    private static bool IsPublicReadWriteProperty(PropertyInfo prop)
    {
        return prop.GetMethod?.IsPublic == true
               && prop.SetMethod?.IsPublic == true;
    }

    private static void DrawObjectProperties(object obj, HashSet<string> excluded)
    {
        var type = obj.GetType();
        var app = EditorApplication.Instance;

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!IsPublicReadWriteProperty(prop)) continue;
            if (excluded.Contains(prop.Name)) continue;

            var propType = prop.PropertyType;
            var label = NiceLabel(prop.Name);

            if (propType == typeof(float))
            {
                float val = (float)prop.GetValue(obj)!;
                if (ImGui.DragFloat(label, ref val, 0.05f))
                    prop.SetValue(obj, val);
                TrackContinuousUndo(app);
            }
            else if (propType == typeof(int))
            {
                int val = (int)prop.GetValue(obj)!;
                if (ImGui.DragInt(label, ref val))
                    prop.SetValue(obj, val);
                TrackContinuousUndo(app);
            }
            else if (propType == typeof(bool))
            {
                bool val = (bool)prop.GetValue(obj)!;
                if (ImGui.Checkbox(label, ref val))
                {
                    app.RecordUndo();
                    prop.SetValue(obj, val);
                    app.RefreshUndoBaseline();
                }
            }
            else if (propType == typeof(string))
            {
                string val = (string)(prop.GetValue(obj) ?? "");
                if (ImGui.InputText(label, ref val, 256))
                    prop.SetValue(obj, val);
                TrackContinuousUndo(app);
            }
            else if (propType == typeof(Vector3))
            {
                var val = (Vector3)prop.GetValue(obj)!;
                if (DrawColoredVector3(label, ref val, 0.1f))
                    prop.SetValue(obj, val);
            }
            else if (propType == typeof(Vector2))
            {
                var val = (Vector2)prop.GetValue(obj)!;
                if (ImGui.DragFloat2(label, ref val, 0.1f))
                    prop.SetValue(obj, val);
                TrackContinuousUndo(app);
            }
            else if (propType == typeof(Color))
            {
                DrawColorEdit4(label, (Color)prop.GetValue(obj)!, v => prop.SetValue(obj, v));
            }
            else if (propType.IsEnum)
            {
                object currentValue = prop.GetValue(obj) ?? Enum.GetValues(propType).GetValue(0)!;
                if (ComboEnumHelper.Combo(label, propType, ref currentValue))
                {
                    app.RecordUndo();
                    prop.SetValue(obj, currentValue);
                    app.RefreshUndoBaseline();
                }
            }
            else if (propType == typeof(EntityReference))
            {
                DrawFObjectEntityReference(label, obj, prop);
            }
            else if (propType == typeof(AssetReference))
            {
                var assetRef = (AssetReference)prop.GetValue(obj)!;
                var filterAttr = prop.GetCustomAttribute<AssetFilterAttribute>();
                var assetFilter = filterAttr?.Filter ?? AssetType.Unknown;
                DrawAssetReference(label, assetRef, assetFilter, v =>
                {
                    prop.SetValue(obj, v);
                });
            }
            else if (typeof(FObject).IsAssignableFrom(propType))
            {
                DrawInlineFObjectProperty(label, obj, prop, propType);
            }
            else if (IsFObjectListType(propType, out var elementType))
            {
                DrawInlineFObjectListProperty(label, obj, prop, elementType!);
            }
            else
            {
                ImGui.LabelText(label, propType.Name);
            }
        }
    }

    private static readonly HashSet<string> FObjectExcludedProperties = new()
    {
        nameof(FObject.DisplayName)
    };

    private static unsafe void DrawFObjectEntityReference(string label, object obj, PropertyInfo prop)
    {
        var app = EditorApplication.Instance;
        var scene = app.CurrentScene;
        var entityRef = (EntityReference)(prop.GetValue(obj) ?? EntityReference.None);

        Entity? resolved = null;
        string preview;
        if (!entityRef.IsValid)
        {
            preview = "(None)";
        }
        else
        {
            resolved = scene?.FindEntityById(entityRef.Id);
            preview = resolved != null ? resolved.Name : "(Missing)";
        }

        ImGui.PushID(label);
        ImGui.Columns(2, (string?)null, false);
        ImGui.SetColumnWidth(0, 80);
        ImGui.Text(label);
        ImGui.NextColumn();

        float availWidth = ImGui.GetContentRegionAvail().X;
        float clearButtonWidth = ImGui.CalcTextSize("X").X + ImGui.GetStyle().FramePadding.X * 2f;
        float comboWidth = availWidth - clearButtonWidth - ImGui.GetStyle().ItemSpacing.X;

        ImGui.SetNextItemWidth(comboWidth);
        if (ImGui.BeginCombo("##ref", preview))
        {
            if (ImGui.Selectable("(None)", !entityRef.IsValid))
            {
                app.RecordUndo();
                prop.SetValue(obj, EntityReference.None);
                app.RefreshUndoBaseline();
            }

            if (scene != null)
            {
                foreach (var entity in scene.Entities)
                {
                    bool isSelected = entityRef.IsValid && entity.Id == entityRef.Id;
                    if (ImGui.Selectable(entity.Name + "##" + entity.Id.ToString("N"), isSelected))
                    {
                        app.RecordUndo();
                        prop.SetValue(obj, new EntityReference(entity));
                        app.RefreshUndoBaseline();
                    }
                }
            }

            ImGui.EndCombo();
        }

        if (ImGui.BeginDragDropTarget())
        {
            ImGuiPayload* payload = ImGui.AcceptDragDropPayload("FRINKY_HIERARCHY_ENTITY");
            if (payload != null && payload->Delivery != 0 && app.DraggedEntityId.HasValue)
            {
                var draggedEntity = app.FindEntityById(app.DraggedEntityId.Value);
                if (draggedEntity != null)
                {
                    app.RecordUndo();
                    prop.SetValue(obj, new EntityReference(draggedEntity));
                    app.RefreshUndoBaseline();
                }
            }
            ImGui.EndDragDropTarget();
        }

        ImGui.SameLine();
        if (ImGui.Button("X") && entityRef.IsValid)
        {
            app.RecordUndo();
            prop.SetValue(obj, EntityReference.None);
            app.RefreshUndoBaseline();
        }

        ImGui.Columns(1);
        ImGui.PopID();
    }

    // ─── FObject helpers ──────────────────────────────────────────────

    private static bool IsFObjectListType(Type type, out Type? elementType)
    {
        elementType = null;
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            var arg = type.GetGenericArguments()[0];
            if (typeof(FObject).IsAssignableFrom(arg))
            {
                elementType = arg;
                return true;
            }
        }
        return false;
    }

    private static void DrawFObjectProperty(string label, Component component, PropertyInfo prop, Type propType)
    {
        var app = EditorApplication.Instance;
        var current = prop.GetValue(component) as FObject;
        var candidateTypes = FObjectTypeResolver.GetTypesAssignableTo(propType).ToList();

        string preview = current != null ? current.DisplayName : "(None)";

        ImGui.PushID(prop.Name);

        var flags = ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.AllowOverlap
                  | ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.FramePadding;
        if (current == null)
            flags |= ImGuiTreeNodeFlags.Leaf;

        bool open = ImGui.TreeNodeEx(label, flags);

        // Type dropdown overlaid on the right (same pattern as PostProcess header buttons)
        float comboWidth = ImGui.GetContentRegionAvail().X * 0.55f;
        ImGui.SameLine(ImGui.GetContentRegionAvail().X - comboWidth + ImGui.GetCursorPosX());
        ImGui.SetNextItemWidth(comboWidth);
        if (ImGui.BeginCombo("##type", preview))
        {
            if (ImGui.Selectable("(None)", current == null))
            {
                app.RecordUndo();
                DisposeIfNeeded(current);
                prop.SetValue(component, null);
                app.RefreshUndoBaseline();
            }

            foreach (var type in candidateTypes)
            {
                var displayName = FObjectTypeResolver.GetDisplayName(type);
                var source = FObjectTypeResolver.GetAssemblySource(type);
                var itemLabel = source == "Engine" ? displayName : $"{displayName} ({source})";
                bool isSelected = current != null && current.GetType() == type;

                if (ImGui.Selectable(itemLabel, isSelected))
                {
                    try
                    {
                        var newObj = (FObject)Activator.CreateInstance(type)!;
                        app.RecordUndo();
                        DisposeIfNeeded(current);
                        prop.SetValue(component, newObj);
                        app.RefreshUndoBaseline();
                    }
                    catch { }
                }
            }

            ImGui.EndCombo();
        }

        if (open)
        {
            if (current != null)
            {
                DrawIndentedWithAccentBar(() => DrawObjectProperties(current, FObjectExcludedProperties));
            }
            ImGui.TreePop();
        }

        ImGui.PopID();
    }

    private static void DrawFObjectListProperty(string label, Component component, PropertyInfo prop, Type elementType)
    {
        var app = EditorApplication.Instance;
        var list = prop.GetValue(component) as System.Collections.IList;
        if (list == null)
        {
            ImGui.LabelText(label, "(null)");
            return;
        }

        ImGui.PushID(prop.Name);

        var listLabel = list.Count > 0 ? $"{label} ({list.Count})" : label;
        var listFlags = ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.FramePadding;
        if (list.Count == 0) listFlags |= ImGuiTreeNodeFlags.Leaf;

        bool listOpen = ImGui.TreeNodeEx(listLabel, listFlags);

        if (listOpen)
        {
            DrawIndentedWithAccentBar(() => DrawFObjectListContents(list, prop.Name, elementType, app));
            ImGui.TreePop();
        }

        ImGui.PopID();
    }

    private static void DrawFObjectListContents(System.Collections.IList list, string propName, Type elementType, EditorApplication app)
    {
        int? removeIndex = null;
        int? moveUpIndex = null;
        int? moveDownIndex = null;

        for (int i = 0; i < list.Count; i++)
        {
            var item = list[i] as FObject;
            if (item == null) continue;

            ImGui.PushID(i);

            bool open = ImGui.TreeNodeEx(item.DisplayName, ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.AllowOverlap | ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.FramePadding);

            float buttonSize = ImGui.GetFrameHeight();
            float totalButtonWidth = buttonSize * 3f + ImGui.GetStyle().ItemSpacing.X * 2f;
            ImGui.SameLine(ImGui.GetContentRegionAvail().X - totalButtonWidth + ImGui.GetCursorPosX());

            var transparent = new Vector4(0, 0, 0, 0);
            ImGui.PushStyleColor(ImGuiCol.Button, transparent);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ImGui.GetStyle().Colors[(int)ImGuiCol.HeaderHovered]);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, ImGui.GetStyle().Colors[(int)ImGuiCol.HeaderActive]);
            ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled]);

            if (i > 0)
            {
                if (ImGui.Button("^", new Vector2(buttonSize, buttonSize))) moveUpIndex = i;
            }
            else
            {
                ImGui.BeginDisabled();
                ImGui.Button("^", new Vector2(buttonSize, buttonSize));
                ImGui.EndDisabled();
            }
            ImGui.SameLine();

            if (i < list.Count - 1)
            {
                if (ImGui.Button("v", new Vector2(buttonSize, buttonSize))) moveDownIndex = i;
            }
            else
            {
                ImGui.BeginDisabled();
                ImGui.Button("v", new Vector2(buttonSize, buttonSize));
                ImGui.EndDisabled();
            }
            ImGui.SameLine();

            ImGui.PopStyleColor(); // pop TextDisabled text
            ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.Text]);
            ImGui.PopStyleColor(2); // pop HeaderHovered, HeaderActive
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.8f, 0.2f, 0.2f, 0.4f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.8f, 0.2f, 0.2f, 0.6f));
            if (ImGui.Button("X", new Vector2(buttonSize, buttonSize))) removeIndex = i;
            ImGui.PopStyleColor(4); // pop remaining 4 (transparent Button, red Hovered, red Active, Text)

            if (open)
            {
                DrawIndentedWithAccentBar(() => DrawObjectProperties(item, FObjectExcludedProperties));
                ImGui.TreePop();
            }

            ImGui.PopID();
            ImGui.Spacing();
        }

        if (removeIndex.HasValue)
        {
            app.RecordUndo();
            DisposeIfNeeded(list[removeIndex.Value]);
            list.RemoveAt(removeIndex.Value);
            app.RefreshUndoBaseline();
        }
        else if (moveUpIndex.HasValue)
        {
            app.RecordUndo();
            int idx = moveUpIndex.Value;
            (list[idx], list[idx - 1]) = (list[idx - 1], list[idx]);
            app.RefreshUndoBaseline();
        }
        else if (moveDownIndex.HasValue)
        {
            app.RecordUndo();
            int idx = moveDownIndex.Value;
            (list[idx], list[idx + 1]) = (list[idx + 1], list[idx]);
            app.RefreshUndoBaseline();
        }

        ImGui.Spacing();

        var popupId = $"AddFObject_{propName}";
        float availWidth = ImGui.GetContentRegionAvail().X;
        float buttonWidthAdd = MathF.Min(200f, availWidth);
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (availWidth - buttonWidthAdd) * 0.5f);
        if (ImGui.Button($"Add {NiceLabel(elementType.Name)}", new Vector2(buttonWidthAdd, 0)))
            ImGui.OpenPopup(popupId);

        if (ImGui.BeginPopup(popupId))
        {
            var candidateTypes = FObjectTypeResolver.GetTypesAssignableTo(elementType).ToList();
            if (candidateTypes.Count == 0)
            {
                ImGui.TextDisabled("No types available");
            }
            else
            {
                foreach (var type in candidateTypes)
                {
                    var displayName = FObjectTypeResolver.GetDisplayName(type);
                    var source = FObjectTypeResolver.GetAssemblySource(type);
                    var itemLabel = source == "Engine" ? displayName : $"{displayName} ({source})";

                    if (ImGui.Selectable(itemLabel))
                    {
                        try
                        {
                            var newObj = (FObject)Activator.CreateInstance(type)!;
                            app.RecordUndo();
                            list.Add(newObj);
                            app.RefreshUndoBaseline();
                        }
                        catch { }
                    }
                }
            }
            ImGui.EndPopup();
        }
    }

    private static void DrawInlineFObjectProperty(string label, object owner, PropertyInfo prop, Type propType)
    {
        var app = EditorApplication.Instance;
        var current = prop.GetValue(owner) as FObject;
        var candidateTypes = FObjectTypeResolver.GetTypesAssignableTo(propType).ToList();

        string preview = current != null ? current.DisplayName : "(None)";

        ImGui.PushID(prop.Name);

        var flags = ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.AllowOverlap
                  | ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.FramePadding;
        if (current == null)
            flags |= ImGuiTreeNodeFlags.Leaf;

        bool open = ImGui.TreeNodeEx(label, flags);

        // Type dropdown overlaid on the right (same pattern as PostProcess header buttons)
        float comboWidth = ImGui.GetContentRegionAvail().X * 0.55f;
        ImGui.SameLine(ImGui.GetContentRegionAvail().X - comboWidth + ImGui.GetCursorPosX());
        ImGui.SetNextItemWidth(comboWidth);
        if (ImGui.BeginCombo("##type", preview))
        {
            if (ImGui.Selectable("(None)", current == null))
            {
                app.RecordUndo();
                DisposeIfNeeded(current);
                prop.SetValue(owner, null);
                app.RefreshUndoBaseline();
            }

            foreach (var type in candidateTypes)
            {
                var displayName = FObjectTypeResolver.GetDisplayName(type);
                var source = FObjectTypeResolver.GetAssemblySource(type);
                var itemLabel = source == "Engine" ? displayName : $"{displayName} ({source})";
                bool isSelected = current != null && current.GetType() == type;

                if (ImGui.Selectable(itemLabel, isSelected))
                {
                    try
                    {
                        var newObj = (FObject)Activator.CreateInstance(type)!;
                        app.RecordUndo();
                        DisposeIfNeeded(current);
                        prop.SetValue(owner, newObj);
                        app.RefreshUndoBaseline();
                    }
                    catch { }
                }
            }

            ImGui.EndCombo();
        }

        if (open)
        {
            if (current != null)
            {
                DrawIndentedWithAccentBar(() => DrawObjectProperties(current, FObjectExcludedProperties));
            }
            ImGui.TreePop();
        }

        ImGui.PopID();
    }

    private static void DrawInlineFObjectListProperty(string label, object owner, PropertyInfo prop, Type elementType)
    {
        var app = EditorApplication.Instance;
        var list = prop.GetValue(owner) as System.Collections.IList;
        if (list == null)
        {
            ImGui.LabelText(label, "(null)");
            return;
        }

        ImGui.PushID(prop.Name);

        var listLabel = list.Count > 0 ? $"{label} ({list.Count})" : label;
        var listFlags = ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.FramePadding;
        if (list.Count == 0) listFlags |= ImGuiTreeNodeFlags.Leaf;

        bool listOpen = ImGui.TreeNodeEx(listLabel, listFlags);

        if (listOpen)
        {
            DrawIndentedWithAccentBar(() => DrawFObjectListContents(list, $"{prop.Name}_inline", elementType, app));
            ImGui.TreePop();
        }

        ImGui.PopID();
    }

    private static void DisposeIfNeeded(object? value)
    {
        if (value is not IDisposable disposable)
            return;

        try { disposable.Dispose(); }
        catch { }
    }

    // ─── Reusable helpers ───────────────────────────────────────────────

    private static readonly Vector4 AccentBarColor = new(0.35f, 0.55f, 0.75f, 0.45f);

    private static void DrawIndentedWithAccentBar(Action drawContent)
    {
        float indentSpacing = ImGui.GetStyle().IndentSpacing;
        ImGui.Indent();

        var startPos = ImGui.GetCursorScreenPos();
        float startY = startPos.Y;

        drawContent();

        float endY = ImGui.GetCursorScreenPos().Y;
        ImGui.Unindent();

        if (endY > startY)
        {
            var drawList = ImGui.GetWindowDrawList();
            float barX = startPos.X - indentSpacing + 4f;
            drawList.AddRectFilled(
                new Vector2(barX, startY),
                new Vector2(barX + 2f, endY),
                ImGui.GetColorU32(AccentBarColor));
        }
    }

    private static readonly Vector4 WarningColor = new(1f, 0.55f, 0.25f, 1f);

    private static readonly Regex PascalCaseRegex = new(@"(?<=[a-z0-9])([A-Z])|(?<=[A-Z])([A-Z][a-z])", RegexOptions.Compiled);

    private static string NiceLabel(string propertyName)
    {
        return PascalCaseRegex.Replace(propertyName, " $1$2");
    }

    private static void DrawSection(string title)
    {
        ImGui.SeparatorText(title);
    }

    private static void DrawWarning(string message)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, WarningColor);
        ImGui.TextWrapped(message);
        ImGui.PopStyleColor();
    }

    private static void DrawReadOnlyText(string label, string value)
    {
        ImGui.TextUnformatted($"{label}: {value}");
    }

    private static bool DrawDragFloat(string label, ref float value, float speed = 0.05f, float min = 0f, float max = 0f)
    {
        bool changed = ImGui.DragFloat(label, ref value, speed, min, max);
        TrackContinuousUndo(EditorApplication.Instance);
        return changed;
    }

    private static void DrawCheckbox(string label, bool currentValue, Action<bool> setter)
    {
        bool val = currentValue;
        if (ImGui.Checkbox(label, ref val))
        {
            var app = EditorApplication.Instance;
            app.RecordUndo();
            setter(val);
            app.RefreshUndoBaseline();
        }
    }

    private static void DrawEnumCombo<T>(string label, T currentValue, Action<T> setter) where T : struct, Enum
    {
        var value = currentValue;
        if (ComboEnumHelper<T>.Combo(label, ref value))
        {
            var app = EditorApplication.Instance;
            app.RecordUndo();
            setter(value);
            app.RefreshUndoBaseline();
        }
    }

    private static readonly Dictionary<string, string> _searchFilters = new();

    private static void DrawSearchableEnumCombo<T>(string label, T currentValue, Action<T> setter) where T : struct, Enum
    {
        var filterId = label;
        if (!_searchFilters.ContainsKey(filterId))
            _searchFilters[filterId] = "";

        var preview = currentValue.ToString();
        if (ImGui.BeginCombo(label, preview))
        {
            var filter = _searchFilters[filterId];
            ImGui.InputTextWithHint("##filter", "Search...", ref filter, 64);
            _searchFilters[filterId] = filter;

            var values = Enum.GetValues<T>();
            var filterLower = filter.ToLowerInvariant();
            foreach (var value in values)
            {
                var name = value.ToString();
                if (filter.Length > 0 && !name.ToLowerInvariant().Contains(filterLower))
                    continue;

                bool isSelected = EqualityComparer<T>.Default.Equals(value, currentValue);
                if (ImGui.Selectable(name, isSelected))
                {
                    var app = EditorApplication.Instance;
                    app.RecordUndo();
                    setter(value);
                    app.RefreshUndoBaseline();
                }
                if (isSelected)
                    ImGui.SetItemDefaultFocus();
            }
            ImGui.EndCombo();
        }
        else
        {
            _searchFilters[filterId] = "";
        }
    }

    private static bool DrawSearchableEnumCombo(string label, Type enumType, ref object currentValue)
    {
        if (!enumType.IsEnum)
            return false;

        var filterId = $"{label}_{enumType.FullName}";
        if (!_searchFilters.ContainsKey(filterId))
            _searchFilters[filterId] = "";

        var preview = currentValue?.ToString() ?? "(None)";
        bool changed = false;
        if (ImGui.BeginCombo(label, preview))
        {
            var filter = _searchFilters[filterId];
            ImGui.InputTextWithHint("##filter", "Search...", ref filter, 64);
            _searchFilters[filterId] = filter;

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
            _searchFilters[filterId] = "";
        }

        return changed;
    }

    private static void DrawColorEdit4(string label, Color currentValue, Action<Color> setter)
    {
        var color = ColorToVec4(currentValue);
        if (ImGui.ColorEdit4(label, ref color))
            setter(Vec4ToColor(color));
        TrackContinuousUndo(EditorApplication.Instance);
    }

    // ─── End reusable helpers ─────────────────────────────────────────

    private static void TrackContinuousUndo(EditorApplication app)
    {
        if (ImGui.IsItemActivated())
            app.RecordUndo();
        if (ImGui.IsItemDeactivatedAfterEdit())
            app.RefreshUndoBaseline();
    }

    private static Vector4 ColorToVec4(Color c) =>
        new(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);

    private static Color Vec4ToColor(Vector4 v) =>
        new((byte)(v.X * 255), (byte)(v.Y * 255), (byte)(v.Z * 255), (byte)(v.W * 255));

    private static Vector4 ParseHexColor(string hex)
    {
        if (string.IsNullOrEmpty(hex))
            return new Vector4(1, 1, 1, 1);

        hex = hex.TrimStart('#');
        if (hex.Length >= 6 &&
            int.TryParse(hex[..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int r) &&
            int.TryParse(hex[2..4], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int g) &&
            int.TryParse(hex[4..6], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int b))
        {
            return new Vector4(r / 255f, g / 255f, b / 255f, 1f);
        }

        return new Vector4(1, 1, 1, 1);
    }
}
