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
    private static readonly Vector4 ColorRed = new(0.867f, 0.2f, 0.267f, 1f);    // #DD3344
    private static readonly Vector4 ColorRedHover = new(0.933f, 0.3f, 0.367f, 1f);
    private static readonly Vector4 ColorGreen = new(0.267f, 0.733f, 0.267f, 1f);  // #44BB44
    private static readonly Vector4 ColorGreenHover = new(0.367f, 0.833f, 0.367f, 1f);
    private static readonly Vector4 ColorBlue = new(0.267f, 0.533f, 0.867f, 1f);   // #4488DD
    private static readonly Vector4 ColorBlueHover = new(0.367f, 0.633f, 0.933f, 1f);

    private static bool DrawColoredVector3(string label, ref Vector3 value, float speed)
    {
        return DrawColoredVector3(label, ref value, speed, Vector3.Zero);
    }

    private static bool DrawColoredVector3(string label, ref Vector3 value, float speed, float resetValue)
    {
        return DrawColoredVector3(label, ref value, speed, new Vector3(resetValue));
    }

    private static bool DrawColoredVector3(string label, ref Vector3 value, float speed, Vector3 resetValue)
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
            value.X = resetValue.X;
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
            value.Y = resetValue.Y;
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
            value.Z = resetValue.Z;
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


    public static void DrawReflection(Component component)
    {
        SyncInspectorCachesForScene();

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

        DrawInspectorMessages(component);
        DrawInspectorButtons(component);
        DrawTransientMessages(component);
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
        void NotifyPropertyChanged() => InvokeOnChangedCallbacks(component, prop);

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
                {
                    prop.SetValue(component, val);
                    NotifyPropertyChanged();
                }
                TrackContinuousUndo(app);
            });
        }
        else if (propType == typeof(int))
        {
            var dropdownAttr = prop.GetCustomAttribute<InspectorDropdownAttribute>();
            if (dropdownAttr != null)
            {
                int val = (int)prop.GetValue(component)!;
                string[]? options = null;
                var method = component.GetType().GetMethod(dropdownAttr.MethodName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                    binder: null, types: Type.EmptyTypes, modifiers: null);
                if (method != null && method.ReturnType == typeof(string[]))
                    options = method.Invoke(component, null) as string[];

                options ??= [];
                string preview = val >= 0 && val < options.Length ? options[val] : $"{val}";
                DrawPropertyWithTooltip(prop, () =>
                {
                    if (ImGui.BeginCombo(label, preview))
                    {
                        for (int i = 0; i < options.Length; i++)
                        {
                            bool selected = i == val;
                            if (ImGui.Selectable(options[i], selected))
                            {
                                prop.SetValue(component, i);
                                NotifyPropertyChanged();
                            }
                            if (selected)
                                ImGui.SetItemDefaultFocus();
                        }
                        ImGui.EndCombo();
                    }
                    TrackContinuousUndo(app);
                });
            }
            else
            {
                int val = (int)prop.GetValue(component)!;
                float speed = rangeAttr?.Speed ?? 1f;
                int min = (int)(rangeAttr?.Min ?? int.MinValue);
                int max = (int)(rangeAttr?.Max ?? int.MaxValue);
                DrawPropertyWithTooltip(prop, () =>
                {
                    if (ImGui.DragInt(label, ref val, speed, min, max))
                    {
                        prop.SetValue(component, val);
                        NotifyPropertyChanged();
                    }
                    TrackContinuousUndo(app);
                });
            }
        }
        else if (propType == typeof(bool))
        {
            DrawPropertyWithTooltip(prop, () =>
                DrawCheckbox(label, (bool)prop.GetValue(component)!, v =>
                {
                    prop.SetValue(component, v);
                    NotifyPropertyChanged();
                }));
        }
        else if (propType == typeof(string))
        {
            string val = (string)(prop.GetValue(component) ?? "");
            DrawPropertyWithTooltip(prop, () =>
            {
                if (ImGui.InputText(label, ref val, 256))
                {
                    prop.SetValue(component, val);
                    NotifyPropertyChanged();
                }
                TrackContinuousUndo(app);
            });
        }
        else if (propType == typeof(Vector3))
        {
            var val = (Vector3)prop.GetValue(component)!;
            var styleAttr = prop.GetCustomAttribute<InspectorVector3StyleAttribute>();
            var reset = styleAttr != null
                ? new Vector3(styleAttr.ResetX, styleAttr.ResetY, styleAttr.ResetZ)
                : Vector3.Zero;
            bool useColoredVector =
                styleAttr == null
                || styleAttr.Style == InspectorVector3Style.ColoredAxisReset;

            DrawPropertyWithTooltip(prop, () =>
            {
                bool changed = useColoredVector
                    ? DrawColoredVector3(label, ref val, 0.1f, reset)
                    : ImGui.DragFloat3(label, ref val, 0.1f);

                if (changed)
                {
                    prop.SetValue(component, val);
                    NotifyPropertyChanged();
                }

                if (!useColoredVector)
                    TrackContinuousUndo(app);
            });
        }
        else if (propType == typeof(Vector2))
        {
            var val = (Vector2)prop.GetValue(component)!;
            DrawPropertyWithTooltip(prop, () =>
            {
                if (ImGui.DragFloat2(label, ref val, 0.1f))
                {
                    prop.SetValue(component, val);
                    NotifyPropertyChanged();
                }
                TrackContinuousUndo(app);
            });
        }
        else if (propType == typeof(Quaternion))
        {
            var q = (Quaternion)prop.GetValue(component)!;
            var euler = Core.FrinkyMath.QuaternionToEuler(q);
            DrawPropertyWithTooltip(prop, () =>
            {
                if (DrawColoredVector3(label, ref euler, 0.5f, Vector3.Zero))
                {
                    prop.SetValue(component, Core.FrinkyMath.EulerToQuaternion(euler));
                    NotifyPropertyChanged();
                }
            });
        }
        else if (propType == typeof(Color))
        {
            DrawPropertyWithTooltip(prop, () =>
                DrawColorEdit4(label, (Color)prop.GetValue(component)!, v =>
                {
                    prop.SetValue(component, v);
                    NotifyPropertyChanged();
                }));
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
                    NotifyPropertyChanged();
                }
            });
        }
        else if (propType == typeof(EntityReference))
        {
            if (DrawEntityReference(label, component, prop))
                NotifyPropertyChanged();
        }
        else if (propType == typeof(AssetReference))
        {
            var assetRef = (AssetReference)prop.GetValue(component)!;
            var filterAttr = prop.GetCustomAttribute<AssetFilterAttribute>();
            var assetFilter = filterAttr?.Filter ?? AssetType.Unknown;
            DrawAssetReference(label, assetRef, assetFilter, v =>
            {
                prop.SetValue(component, v);
                NotifyPropertyChanged();
            });
        }
        else if (propType == typeof(AudioAttenuationSettings) && component is AudioSourceComponent audioSource)
        {
            DrawAudioAttenuationSettings(audioSource, prop, NotifyPropertyChanged);
        }
        else if (typeof(FObject).IsAssignableFrom(propType))
        {
            DrawFObjectProperty(label, component, prop, propType, NotifyPropertyChanged);
        }
        else if (IsFObjectListType(propType, out var fobjectElementType))
        {
            DrawFObjectListProperty(label, component, prop, fobjectElementType!, NotifyPropertyChanged);
        }
        else if (IsListType(propType, out var listElementType))
        {
            DrawInlineListProperty(label, component, prop, listElementType!, NotifyPropertyChanged);
        }
        else if (IsInlineObjectType(propType) && prop.GetCustomAttribute<InspectorInlineAttribute>() != null)
        {
            DrawInlineFlatProperties(component, prop, propType, NotifyPropertyChanged);
        }
        else if (IsInlineObjectType(propType))
        {
            DrawInlineObjectProperty(label, component, prop, propType, NotifyPropertyChanged);
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

    private static void DrawAudioAttenuationSettings(AudioSourceComponent source, PropertyInfo prop, Action notifyPropertyChanged)
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

            object rolloffValue = attenuation.Rolloff;
            if (ComboEnumHelper.Combo("Rolloff", typeof(AudioRolloffMode), ref rolloffValue))
            {
                app.RecordUndo();
                attenuation.Rolloff = (AudioRolloffMode)rolloffValue;
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

        if (!changed)
            return;

        prop.SetValue(source, attenuation);
        notifyPropertyChanged();
    }

    private static bool IsInspectableProperty(PropertyInfo prop)
    {
        return InspectorReflectionHelpers.IsInspectableComponentProperty(prop);
    }

    private static bool IsInspectableObjectProperty(PropertyInfo prop)
    {
        if (!prop.CanRead)
            return false;
        if (prop.GetCustomAttribute<InspectorHiddenAttribute>() != null)
            return false;
        if (prop.CanWrite)
            return true;
        return prop.GetCustomAttribute<InspectorReadOnlyAttribute>() != null;
    }

    private static bool IsPropertyVisible(object owner, PropertyInfo prop)
    {
        foreach (var visibleIf in prop.GetCustomAttributes<InspectorVisibleIfAttribute>())
        {
            if (!InspectorReflectionHelpers.TryEvaluateBoolMember(owner, visibleIf.PropertyName, out var current))
                return false;
            if (current != visibleIf.ExpectedValue)
                return false;
        }

        foreach (var visibleIfEnum in prop.GetCustomAttributes<InspectorVisibleIfEnumAttribute>())
        {
            if (!InspectorReflectionHelpers.TryEvaluateEnumMember(owner, visibleIfEnum.PropertyName, out var enumValue))
                return false;
            if (!string.Equals(enumValue?.ToString(), visibleIfEnum.ExpectedMemberName, StringComparison.Ordinal))
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

    private static void DrawReadOnlyValue(object owner, PropertyInfo prop, string label)
    {
        var value = prop.GetValue(owner);
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

    private static void DrawInspectorMessages(Component component)
    {
        var type = component.GetType();
        var messages = type
            .GetCustomAttributes<InspectorMessageIfAttribute>(inherit: true)
            .OrderBy(attribute => attribute.Order)
            .ToList();

        if (messages.Count == 0)
            return;

        foreach (var messageAttr in messages)
        {
            if (!IsModeVisible(messageAttr.Mode))
                continue;
            if (!InspectorReflectionHelpers.TryEvaluateBoolMember(component, messageAttr.ConditionMember, out var show) || !show)
                continue;

            ImGui.Spacing();
            DrawMessage(messageAttr.Message, messageAttr.Severity);
        }
    }

    private static void DrawInspectorButtons(Component component)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        var methods = component
            .GetType()
            .GetMethods(flags)
            .Select(method => new
            {
                Method = method,
                Attribute = method.GetCustomAttribute<InspectorButtonAttribute>(inherit: true)
            })
            .Where(item => item.Attribute != null)
            .Select(item => new InspectorButtonEntry(item.Method, item.Attribute!))
            .Where(entry => entry.Method.GetParameters().Length == 0)
            .Where(entry => entry.Method.ReturnType == typeof(void) || entry.Method.ReturnType == typeof(bool))
            .Where(entry => IsModeVisible(entry.Attribute.Mode))
            .OrderBy(entry => entry.Attribute.Order)
            .ThenBy(entry => entry.Method.MetadataToken)
            .ToList();

        if (methods.Count == 0)
            return;

        ImGui.Spacing();
        string? lastSection = null;
        foreach (var entry in methods)
        {
            if (!string.IsNullOrWhiteSpace(entry.Attribute.Section)
                && !string.Equals(lastSection, entry.Attribute.Section, StringComparison.Ordinal))
            {
                DrawSection(entry.Attribute.Section!);
                lastSection = entry.Attribute.Section;
            }

            bool disable = false;
            if (!string.IsNullOrWhiteSpace(entry.Attribute.DisableWhen))
            {
                if (InspectorReflectionHelpers.TryEvaluateBoolMember(component, entry.Attribute.DisableWhen!, out var disableState))
                    disable = disableState;
            }

            ImGui.BeginDisabled(disable);
            bool clicked = ImGui.Button(entry.Attribute.Label);
            ImGui.EndDisabled();
            if (!clicked)
                continue;

            bool shouldTrackUndo = EditorApplication.Instance.CanEditScene;
            if (shouldTrackUndo)
                EditorApplication.Instance.RecordUndo();

            try
            {
                object? result = entry.Method.Invoke(component, null);

                if (entry.Method.ReturnType == typeof(bool) && result is bool ok && !ok)
                    EnqueueTransientMessage(component, $"'{entry.Attribute.Label}' returned false.", InspectorMessageSeverity.Info);
            }
            catch (Exception ex)
            {
                FrinkyLog.Error($"InspectorButton '{entry.Attribute.Label}' failed on {component.GetType().Name}: {ex.InnerException?.Message ?? ex.Message}");
            }
            finally
            {
                if (shouldTrackUndo)
                    EditorApplication.Instance.RefreshUndoBaseline();
            }
        }
    }

    public static void InvokeOnChangedCallbacks(Component component, PropertyInfo prop)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        foreach (var callback in prop.GetCustomAttributes<InspectorOnChangedAttribute>(inherit: true))
        {
            var callbackKey = $"{component.GetType().FullName}:{prop.Name}:{callback.MethodName}";
            var method = component.GetType().GetMethod(callback.MethodName, flags, binder: null, types: Type.EmptyTypes, modifiers: null);
            if (method == null)
            {
                if (_loggedOnChangedIssues.Add($"{callbackKey}:missing"))
                    FrinkyLog.Warning($"InspectorOnChanged method '{callback.MethodName}' was not found on {component.GetType().Name} for property '{prop.Name}'.");
                continue;
            }

            if (method.ReturnType != typeof(void))
            {
                if (_loggedOnChangedIssues.Add($"{callbackKey}:return"))
                    FrinkyLog.Warning($"InspectorOnChanged method '{callback.MethodName}' on {component.GetType().Name} must return void.");
                continue;
            }

            try
            {
                method.Invoke(component, null);
            }
            catch (Exception ex)
            {
                FrinkyLog.Error($"InspectorOnChanged callback '{callback.MethodName}' failed on {component.GetType().Name}: {ex.Message}");
            }
        }
    }

    private static bool IsModeVisible(InspectorUiMode mode)
    {
        var app = EditorApplication.Instance;
        return mode switch
        {
            InspectorUiMode.Always => true,
            InspectorUiMode.EditorOnly => app.CanEditScene,
            InspectorUiMode.RuntimeOnly => app.IsInRuntimeMode,
            _ => true
        };
    }

    private static void DrawMessage(string message, InspectorMessageSeverity severity)
    {
        switch (severity)
        {
            case InspectorMessageSeverity.Info:
                ImGui.TextDisabled(message);
                break;

            case InspectorMessageSeverity.Warning:
                DrawWarning(message);
                break;

            case InspectorMessageSeverity.Error:
                ImGui.PushStyleColor(ImGuiCol.Text, ErrorColor);
                ImGui.TextWrapped(message);
                ImGui.PopStyleColor();
                break;
        }
    }

    private static void EnqueueTransientMessage(Component component, string message, InspectorMessageSeverity severity)
    {
        var key = GetComponentKey(component);
        if (!_transientMessagesByComponentKey.TryGetValue(key, out var list))
        {
            list = new List<TransientInspectorMessage>();
            _transientMessagesByComponentKey[key] = list;
        }

        list.Add(new TransientInspectorMessage(message, severity, ImGui.GetTime() + 2.0));
    }

    private static void DrawTransientMessages(Component component)
    {
        var key = GetComponentKey(component);
        if (!_transientMessagesByComponentKey.TryGetValue(key, out var list) || list.Count == 0)
            return;

        double now = ImGui.GetTime();
        list.RemoveAll(entry => entry.ExpiresAt <= now);
        if (list.Count == 0)
            return;

        foreach (var entry in list)
            DrawMessage(entry.Message, entry.Severity);
    }

    private static string GetComponentKey(Component component)
    {
        var entityId = component.Entity != null ? component.Entity.Id.ToString("N") : "no-entity";
        return $"{entityId}:{component.GetType().FullName}";
    }

    private static void SyncInspectorCachesForScene()
    {
        var currentScene = EditorApplication.Instance.CurrentScene;
        if (!ReferenceEquals(currentScene, _lastSceneForInspectorCaches))
        {
            _lastSceneForInspectorCaches = currentScene;
            _transientMessagesByComponentKey.Clear();
            _loggedOnChangedIssues.Clear();
            return;
        }

        PruneExpiredTransientMessages(ImGui.GetTime());
    }

    private static void PruneExpiredTransientMessages(double now)
    {
        if (_transientMessagesByComponentKey.Count == 0)
            return;

        var keysToRemove = new List<string>();
        foreach (var (key, entries) in _transientMessagesByComponentKey)
        {
            entries.RemoveAll(entry => entry.ExpiresAt <= now);
            if (entries.Count == 0)
                keysToRemove.Add(key);
        }

        foreach (var key in keysToRemove)
            _transientMessagesByComponentKey.Remove(key);
    }

    private readonly record struct InspectorButtonEntry(MethodInfo Method, InspectorButtonAttribute Attribute);
    private readonly record struct TransientInspectorMessage(string Message, InspectorMessageSeverity Severity, double ExpiresAt);
    private static readonly Dictionary<string, List<TransientInspectorMessage>> _transientMessagesByComponentKey = new();
    private static readonly HashSet<string> _loggedOnChangedIssues = new();
    private static Scene? _lastSceneForInspectorCaches;

    private static readonly Dictionary<string, string> _entityRefFilters = new();

    private static unsafe bool DrawEntityReference(string label, Component component, PropertyInfo prop)
    {
        var app = EditorApplication.Instance;
        var scene = app.CurrentScene;
        var entityRef = (EntityReference)(prop.GetValue(component) ?? EntityReference.None);
        bool changed = false;

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
                changed = true;
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
                        changed = true;
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
                    changed = true;
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
            changed = true;
        }

        ImGui.Columns(1);
        ImGui.PopID();
        return changed;
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

    private const int MaxNestedObjectDepth = 16;
    private static int _nestedObjectDrawDepth;

    private static bool DrawObjectProperties(object obj, HashSet<string> excluded, Action? onChanged = null)
    {
        if (_nestedObjectDrawDepth >= MaxNestedObjectDepth)
        {
            DrawWarning($"Maximum inspector nesting depth ({MaxNestedObjectDepth}) reached. Recursive object graph truncated.");
            return false;
        }

        _nestedObjectDrawDepth++;
        try
        {
            bool anyChanged = false;
            var app = EditorApplication.Instance;
            string? lastSection = null;
            string? lastHeader = null;

            foreach (var prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!IsInspectableObjectProperty(prop))
                    continue;
                if (excluded.Contains(prop.Name))
                    continue;
                if (!IsPropertyVisible(obj, prop))
                    continue;

                var propType = prop.PropertyType;
                var label = GetInspectorLabel(prop);
                bool isReadOnly = !prop.CanWrite || prop.GetCustomAttribute<InspectorReadOnlyAttribute>() != null;

                ApplyLayoutAttributes(prop, ref lastSection, ref lastHeader);

                if (isReadOnly)
                {
                    DrawReadOnlyValue(obj, prop, label);
                    EndLayoutAttributes(prop);
                    continue;
                }

                var rangeAttr = prop.GetCustomAttribute<InspectorRangeAttribute>();

                if (propType == typeof(float))
                {
                    float val = (float)prop.GetValue(obj)!;
                    float speed = rangeAttr?.Speed ?? 0.1f;
                    float min = rangeAttr?.Min ?? float.MinValue;
                    float max = rangeAttr?.Max ?? float.MaxValue;
                    DrawPropertyWithTooltip(prop, () =>
                    {
                        if (ImGui.DragFloat(label, ref val, speed, min, max))
                        {
                            prop.SetValue(obj, val);
                            onChanged?.Invoke();
                            anyChanged = true;
                        }
                        TrackContinuousUndo(app);
                    });
                }
                else if (propType == typeof(int))
                {
                    var dropdownAttr = prop.GetCustomAttribute<InspectorDropdownAttribute>();
                    if (dropdownAttr != null)
                    {
                        int val = (int)prop.GetValue(obj)!;
                        string[]? options = null;
                        var method = obj.GetType().GetMethod(dropdownAttr.MethodName,
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                            binder: null, types: Type.EmptyTypes, modifiers: null);
                        if (method != null && method.ReturnType == typeof(string[]))
                            options = method.Invoke(obj, null) as string[];

                        options ??= [];
                        string preview = val >= 0 && val < options.Length ? options[val] : $"{val}";
                        DrawPropertyWithTooltip(prop, () =>
                        {
                            if (ImGui.BeginCombo(label, preview))
                            {
                                for (int i = 0; i < options.Length; i++)
                                {
                                    bool selected = i == val;
                                    if (ImGui.Selectable(options[i], selected))
                                    {
                                        prop.SetValue(obj, i);
                                        onChanged?.Invoke();
                                        anyChanged = true;
                                    }
                                    if (selected)
                                        ImGui.SetItemDefaultFocus();
                                }
                                ImGui.EndCombo();
                            }
                            TrackContinuousUndo(app);
                        });
                    }
                    else
                    {
                        int val = (int)prop.GetValue(obj)!;
                        float speed = rangeAttr?.Speed ?? 1f;
                        int min = (int)(rangeAttr?.Min ?? int.MinValue);
                        int max = (int)(rangeAttr?.Max ?? int.MaxValue);
                        DrawPropertyWithTooltip(prop, () =>
                        {
                            if (ImGui.DragInt(label, ref val, speed, min, max))
                            {
                                prop.SetValue(obj, val);
                                onChanged?.Invoke();
                                anyChanged = true;
                            }
                            TrackContinuousUndo(app);
                        });
                    }
                }
                else if (propType == typeof(bool))
                {
                    DrawPropertyWithTooltip(prop, () =>
                        DrawCheckbox(label, (bool)prop.GetValue(obj)!, v =>
                        {
                            prop.SetValue(obj, v);
                            onChanged?.Invoke();
                            anyChanged = true;
                        }));
                }
                else if (propType == typeof(string))
                {
                    string val = (string)(prop.GetValue(obj) ?? "");
                    DrawPropertyWithTooltip(prop, () =>
                    {
                        if (ImGui.InputText(label, ref val, 256))
                        {
                            prop.SetValue(obj, val);
                            onChanged?.Invoke();
                            anyChanged = true;
                        }
                        TrackContinuousUndo(app);
                    });
                }
                else if (propType == typeof(Vector3))
                {
                    var val = (Vector3)prop.GetValue(obj)!;
                    var styleAttr = prop.GetCustomAttribute<InspectorVector3StyleAttribute>();
                    var reset = styleAttr != null
                        ? new Vector3(styleAttr.ResetX, styleAttr.ResetY, styleAttr.ResetZ)
                        : Vector3.Zero;
                    bool useColoredVector =
                        styleAttr == null
                        || styleAttr.Style == InspectorVector3Style.ColoredAxisReset;

                    DrawPropertyWithTooltip(prop, () =>
                    {
                        bool changed = useColoredVector
                            ? DrawColoredVector3(label, ref val, 0.1f, reset)
                            : ImGui.DragFloat3(label, ref val, 0.1f);

                        if (changed)
                        {
                            prop.SetValue(obj, val);
                            onChanged?.Invoke();
                            anyChanged = true;
                        }

                        if (!useColoredVector)
                            TrackContinuousUndo(app);
                    });
                }
                else if (propType == typeof(Vector2))
                {
                    var val = (Vector2)prop.GetValue(obj)!;
                    DrawPropertyWithTooltip(prop, () =>
                    {
                        if (ImGui.DragFloat2(label, ref val, 0.1f))
                        {
                            prop.SetValue(obj, val);
                            onChanged?.Invoke();
                            anyChanged = true;
                        }
                        TrackContinuousUndo(app);
                    });
                }
                else if (propType == typeof(Quaternion))
                {
                    var q = (Quaternion)prop.GetValue(obj)!;
                    var euler = Core.FrinkyMath.QuaternionToEuler(q);
                    DrawPropertyWithTooltip(prop, () =>
                    {
                        if (DrawColoredVector3(label, ref euler, 0.5f, Vector3.Zero))
                        {
                            prop.SetValue(obj, Core.FrinkyMath.EulerToQuaternion(euler));
                            onChanged?.Invoke();
                            anyChanged = true;
                        }
                    });
                }
                else if (propType == typeof(Color))
                {
                    DrawPropertyWithTooltip(prop, () =>
                        DrawColorEdit4(label, (Color)prop.GetValue(obj)!, v =>
                        {
                            prop.SetValue(obj, v);
                            onChanged?.Invoke();
                            anyChanged = true;
                        }));
                }
                else if (propType.IsEnum)
                {
                    object currentValue = prop.GetValue(obj) ?? Enum.GetValues(propType).GetValue(0)!;
                    DrawPropertyWithTooltip(prop, () =>
                    {
                        bool changed = prop.GetCustomAttribute<InspectorSearchableEnumAttribute>() != null
                            ? DrawSearchableEnumCombo(label, propType, ref currentValue)
                            : ComboEnumHelper.Combo(label, propType, ref currentValue);

                        if (changed)
                        {
                            app.RecordUndo();
                            prop.SetValue(obj, currentValue);
                            app.RefreshUndoBaseline();
                            onChanged?.Invoke();
                            anyChanged = true;
                        }
                    });
                }
                else if (propType == typeof(EntityReference))
                {
                    if (DrawFObjectEntityReference(label, obj, prop))
                    {
                        onChanged?.Invoke();
                        anyChanged = true;
                    }
                }
                else if (propType == typeof(AssetReference))
                {
                    var assetRef = (AssetReference)prop.GetValue(obj)!;
                    var filterAttr = prop.GetCustomAttribute<AssetFilterAttribute>();
                    var assetFilter = filterAttr?.Filter ?? AssetType.Unknown;
                    DrawAssetReference(label, assetRef, assetFilter, v =>
                    {
                        prop.SetValue(obj, v);
                        onChanged?.Invoke();
                        anyChanged = true;
                    });
                }
                else if (typeof(FObject).IsAssignableFrom(propType))
                {
                    DrawInlineFObjectProperty(label, obj, prop, propType, () =>
                    {
                        onChanged?.Invoke();
                        anyChanged = true;
                    });
                }
                else if (IsFObjectListType(propType, out var fobjectElementType))
                {
                    DrawInlineFObjectListProperty(label, obj, prop, fobjectElementType!, () =>
                    {
                        onChanged?.Invoke();
                        anyChanged = true;
                    });
                }
                else if (IsListType(propType, out var listElementType))
                {
                    if (DrawInlineListProperty(label, obj, prop, listElementType!, onChanged))
                        anyChanged = true;
                }
                else if (IsInlineObjectType(propType) && prop.GetCustomAttribute<InspectorInlineAttribute>() != null)
                {
                    if (DrawInlineFlatProperties(obj, prop, propType, onChanged))
                        anyChanged = true;
                }
                else if (IsInlineObjectType(propType))
                {
                    if (DrawInlineObjectProperty(label, obj, prop, propType, onChanged))
                        anyChanged = true;
                }
                else
                {
                    ImGui.LabelText(label, propType.Name);
                }

                EndLayoutAttributes(prop);
            }

            return anyChanged;
        }
        finally
        {
            _nestedObjectDrawDepth--;
        }
    }

    private static readonly HashSet<string> FObjectExcludedProperties = new()
    {
        nameof(FObject.DisplayName)
    };

    private static unsafe bool DrawFObjectEntityReference(string label, object obj, PropertyInfo prop)
    {
        var app = EditorApplication.Instance;
        var scene = app.CurrentScene;
        var entityRef = (EntityReference)(prop.GetValue(obj) ?? EntityReference.None);
        bool changed = false;

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
                changed = true;
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
                        changed = true;
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
                    changed = true;
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
            changed = true;
        }

        ImGui.Columns(1);
        ImGui.PopID();
        return changed;
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

    private static bool IsListType(Type type, out Type? elementType)
    {
        elementType = null;
        if (!InspectorReflectionHelpers.IsListType(type))
            return false;

        elementType = type.GetGenericArguments()[0];
        return true;
    }

    private static bool IsInlineObjectType(Type type)
    {
        return InspectorReflectionHelpers.IsInlineObjectType(type);
    }

    private static void DrawFObjectProperty(string label, Component component, PropertyInfo prop, Type propType, Action? onChanged)
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
                onChanged?.Invoke();
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
                        onChanged?.Invoke();
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
                DrawIndentedWithAccentBar(() => DrawObjectProperties(current, FObjectExcludedProperties, onChanged));
            }
            ImGui.TreePop();
        }

        ImGui.PopID();
    }

    private static void DrawFObjectListProperty(string label, Component component, PropertyInfo prop, Type elementType, Action? onChanged)
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

        bool listOpen = ImGui.TreeNodeEx(listLabel, listFlags);

        if (listOpen)
        {
            DrawIndentedWithAccentBar(() => DrawFObjectListContents(list, prop.Name, elementType, app, onChanged));
            ImGui.TreePop();
        }

        ImGui.PopID();
    }

    private static void DrawFObjectListContents(System.Collections.IList list, string propName, Type elementType, EditorApplication app, Action? onChanged)
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

            var (up, down, rm) = DrawListItemButtons(i, list.Count);
            if (up.HasValue) moveUpIndex = up.Value;
            if (down.HasValue) moveDownIndex = down.Value;
            if (rm.HasValue) removeIndex = rm.Value;

            if (open)
            {
                DrawIndentedWithAccentBar(() => DrawObjectProperties(item, FObjectExcludedProperties, onChanged));
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
            onChanged?.Invoke();
        }
        else if (moveUpIndex.HasValue)
        {
            app.RecordUndo();
            int idx = moveUpIndex.Value;
            (list[idx], list[idx - 1]) = (list[idx - 1], list[idx]);
            app.RefreshUndoBaseline();
            onChanged?.Invoke();
        }
        else if (moveDownIndex.HasValue)
        {
            app.RecordUndo();
            int idx = moveDownIndex.Value;
            (list[idx], list[idx + 1]) = (list[idx + 1], list[idx]);
            app.RefreshUndoBaseline();
            onChanged?.Invoke();
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
                            onChanged?.Invoke();
                        }
                        catch { }
                    }
                }
            }
            ImGui.EndPopup();
        }
    }

    private static void DrawInlineFObjectProperty(string label, object owner, PropertyInfo prop, Type propType, Action? onChanged)
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
                onChanged?.Invoke();
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
                        onChanged?.Invoke();
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
                DrawIndentedWithAccentBar(() => DrawObjectProperties(current, FObjectExcludedProperties, onChanged));
            }
            ImGui.TreePop();
        }

        ImGui.PopID();
    }

    private static void DrawInlineFObjectListProperty(string label, object owner, PropertyInfo prop, Type elementType, Action? onChanged)
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

        bool listOpen = ImGui.TreeNodeEx(listLabel, listFlags);

        if (listOpen)
        {
            DrawIndentedWithAccentBar(() => DrawFObjectListContents(list, $"{prop.Name}_inline", elementType, app, onChanged));
            ImGui.TreePop();
        }

        ImGui.PopID();
    }

    private static readonly HashSet<string> NoExcludedProperties = new();

    private static bool DrawInlineObjectProperty(string label, object owner, PropertyInfo prop, Type propType, Action? onChanged)
    {
        bool changed = false;
        var app = EditorApplication.Instance;
        var current = prop.GetValue(owner);

        ImGui.PushID(prop.Name);

        if (!propType.IsValueType && current == null)
        {
            ImGui.LabelText(label, "(null)");
            if (TryCreateInlineObject(propType, out var created) && ImGui.Button("Create"))
            {
                app.RecordUndo();
                prop.SetValue(owner, created);
                app.RefreshUndoBaseline();
                onChanged?.Invoke();
                changed = true;
            }

            ImGui.PopID();
            return changed;
        }

        var flags = ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.FramePadding;
        bool open = ImGui.TreeNodeEx(label, flags);
        if (open)
        {
            DrawIndentedWithAccentBar(() =>
            {
                if (propType.IsValueType)
                {
                    var boxed = current ?? Activator.CreateInstance(propType)!;
                    if (DrawObjectProperties(boxed, NoExcludedProperties))
                    {
                        prop.SetValue(owner, boxed);
                        onChanged?.Invoke();
                        changed = true;
                    }
                }
                else if (current != null)
                {
                    if (DrawObjectProperties(current, NoExcludedProperties, onChanged))
                        changed = true;
                }
            });
            ImGui.TreePop();
        }

        ImGui.PopID();
        return changed;
    }

    private static bool DrawInlineFlatProperties(object owner, PropertyInfo prop, Type propType, Action? onChanged)
    {
        bool changed = false;
        var current = prop.GetValue(owner);

        if (!propType.IsValueType && current == null)
        {
            ImGui.LabelText(prop.Name, "(null)");
            return false;
        }

        if (propType.IsValueType)
        {
            var boxed = current ?? Activator.CreateInstance(propType)!;
            if (DrawObjectProperties(boxed, NoExcludedProperties))
            {
                prop.SetValue(owner, boxed);
                onChanged?.Invoke();
                changed = true;
            }
        }
        else if (current != null)
        {
            if (DrawObjectProperties(current, NoExcludedProperties, onChanged))
                changed = true;
        }

        return changed;
    }

    private static bool DrawInlineListProperty(string label, object owner, PropertyInfo prop, Type elementType, Action? onChanged)
    {
        bool changed = false;
        var app = EditorApplication.Instance;
        if (prop.GetValue(owner) is not System.Collections.IList list)
        {
            ImGui.LabelText(label, "(null)");
            return false;
        }

        bool fixedSize = prop.GetCustomAttribute<InspectorFixedListSizeAttribute>() != null;

        ImGui.PushID(prop.Name);

        var listLabel = list.Count > 0 ? $"{label} ({list.Count})" : label;
        var flags = ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.FramePadding;

        bool open = ImGui.TreeNodeEx(listLabel, flags);
        if (open)
        {
            DrawIndentedWithAccentBar(() =>
            {
                int? removeIndex = null;
                int? moveUpIndex = null;
                int? moveDownIndex = null;

                for (int i = 0; i < list.Count; i++)
                {
                    ImGui.PushID(i);
                    var itemLabel = $"Element {i}";
                    bool itemOpen;

                    if (!fixedSize)
                    {
                        itemOpen = ImGui.TreeNodeEx(
                            itemLabel,
                            ImGuiTreeNodeFlags.DefaultOpen
                            | ImGuiTreeNodeFlags.AllowOverlap
                            | ImGuiTreeNodeFlags.SpanAvailWidth
                            | ImGuiTreeNodeFlags.FramePadding);

                        var (up, down, rm) = DrawListItemButtons(i, list.Count);
                        if (up.HasValue) moveUpIndex = up.Value;
                        if (down.HasValue) moveDownIndex = down.Value;
                        if (rm.HasValue) removeIndex = rm.Value;
                    }
                    else
                    {
                        itemOpen = ImGui.TreeNodeEx(
                            itemLabel,
                            ImGuiTreeNodeFlags.DefaultOpen
                            | ImGuiTreeNodeFlags.SpanAvailWidth
                            | ImGuiTreeNodeFlags.FramePadding);
                    }

                    if (itemOpen)
                    {
                        DrawIndentedWithAccentBar(() =>
                        {
                            if (DrawListElement(list, i, elementType, onChanged))
                                changed = true;
                        });
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
                    onChanged?.Invoke();
                    changed = true;
                }
                else if (moveUpIndex.HasValue)
                {
                    app.RecordUndo();
                    int idx = moveUpIndex.Value;
                    (list[idx], list[idx - 1]) = (list[idx - 1], list[idx]);
                    app.RefreshUndoBaseline();
                    onChanged?.Invoke();
                    changed = true;
                }
                else if (moveDownIndex.HasValue)
                {
                    app.RecordUndo();
                    int idx = moveDownIndex.Value;
                    (list[idx], list[idx + 1]) = (list[idx + 1], list[idx]);
                    app.RefreshUndoBaseline();
                    onChanged?.Invoke();
                    changed = true;
                }

                if (!fixedSize)
                {
                    ImGui.Spacing();
                    float availWidth = ImGui.GetContentRegionAvail().X;
                    float buttonWidth = MathF.Min(200f, availWidth);
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (availWidth - buttonWidth) * 0.5f);
                    bool canCreate = CanCreateListItem(owner, prop, elementType);
                    ImGui.BeginDisabled(!canCreate);
                    if (ImGui.Button($"Add {NiceLabel(elementType.Name)}", new Vector2(buttonWidth, 0))
                        && canCreate
                        && TryCreateListItem(owner, prop, elementType, out var item))
                    {
                        app.RecordUndo();
                        list.Add(item!);
                        app.RefreshUndoBaseline();
                        onChanged?.Invoke();
                        changed = true;
                    }
                    ImGui.EndDisabled();
                }
            });

            ImGui.TreePop();
        }

        ImGui.PopID();
        return changed;
    }

    private static bool DrawListElement(System.Collections.IList list, int index, Type elementType, Action? onChanged)
    {
        bool changed = false;
        var app = EditorApplication.Instance;
        var current = list[index];

        if (elementType == typeof(float))
        {
            float value = current is float f ? f : 0f;
            if (ImGui.DragFloat("Value", ref value, 0.1f))
            {
                list[index] = value;
                onChanged?.Invoke();
                changed = true;
            }
            TrackContinuousUndo(app);
            return changed;
        }

        if (elementType == typeof(int))
        {
            int value = current is int i ? i : 0;
            if (ImGui.DragInt("Value", ref value, 1f))
            {
                list[index] = value;
                onChanged?.Invoke();
                changed = true;
            }
            TrackContinuousUndo(app);
            return changed;
        }

        if (elementType == typeof(bool))
        {
            bool value = current is bool b && b;
            if (ImGui.Checkbox("Value", ref value))
            {
                app.RecordUndo();
                list[index] = value;
                app.RefreshUndoBaseline();
                onChanged?.Invoke();
                changed = true;
            }
            return changed;
        }

        if (elementType == typeof(string))
        {
            string value = current as string ?? string.Empty;
            if (ImGui.InputText("Value", ref value, 256))
            {
                list[index] = value;
                onChanged?.Invoke();
                changed = true;
            }
            TrackContinuousUndo(app);
            return changed;
        }

        if (elementType == typeof(Vector2))
        {
            var value = current is Vector2 v2 ? v2 : Vector2.Zero;
            if (ImGui.DragFloat2("Value", ref value, 0.1f))
            {
                list[index] = value;
                onChanged?.Invoke();
                changed = true;
            }
            TrackContinuousUndo(app);
            return changed;
        }

        if (elementType == typeof(Vector3))
        {
            var value = current is Vector3 v3 ? v3 : Vector3.Zero;
            if (DrawColoredVector3("Value", ref value, 0.1f, Vector3.Zero))
            {
                list[index] = value;
                onChanged?.Invoke();
                changed = true;
            }
            return changed;
        }

        if (elementType == typeof(Quaternion))
        {
            var value = current is Quaternion q ? q : Quaternion.Identity;
            var euler = Core.FrinkyMath.QuaternionToEuler(value);
            if (DrawColoredVector3("Value", ref euler, 0.5f, Vector3.Zero))
            {
                list[index] = Core.FrinkyMath.EulerToQuaternion(euler);
                onChanged?.Invoke();
                changed = true;
            }
            return changed;
        }

        if (elementType == typeof(Color))
        {
            var value = current is Color color ? color : new Color(255, 255, 255, 255);
            DrawColorEdit4("Value", value, updated =>
            {
                list[index] = updated;
                onChanged?.Invoke();
                changed = true;
            });
            return changed;
        }

        if (elementType.IsEnum)
        {
            object value = current ?? Enum.GetValues(elementType).GetValue(0)!;
            if (ComboEnumHelper.Combo("Value", elementType, ref value))
            {
                app.RecordUndo();
                list[index] = value;
                app.RefreshUndoBaseline();
                onChanged?.Invoke();
                changed = true;
            }
            return changed;
        }

        if (elementType == typeof(AssetReference))
        {
            var assetRef = current is AssetReference reference ? reference : new AssetReference("");
            DrawAssetReference("Value", assetRef, AssetType.Unknown, updated =>
            {
                list[index] = updated;
                onChanged?.Invoke();
                changed = true;
            });
            return changed;
        }

        if (elementType == typeof(EntityReference))
        {
            var fakeProperty = new ListElementPropertyBridge(list, index);
            if (DrawFObjectEntityReference("Value", fakeProperty, ListElementPropertyBridge.ValueProperty))
            {
                onChanged?.Invoke();
                changed = true;
            }
            return changed;
        }

        if (IsInlineObjectType(elementType))
        {
            if (!elementType.IsValueType && current == null)
            {
                ImGui.LabelText("Value", "(null)");
                if (TryCreateInlineObject(elementType, out var created) && ImGui.Button("Create"))
                {
                    app.RecordUndo();
                    list[index] = created;
                    app.RefreshUndoBaseline();
                    onChanged?.Invoke();
                    changed = true;
                }

                return changed;
            }

            if (elementType.IsValueType)
            {
                var boxed = current ?? Activator.CreateInstance(elementType)!;
                if (DrawObjectProperties(boxed, NoExcludedProperties))
                {
                    list[index] = boxed;
                    onChanged?.Invoke();
                    changed = true;
                }
            }
            else if (current != null && DrawObjectProperties(current, NoExcludedProperties, onChanged))
            {
                changed = true;
            }

            return changed;
        }

        ImGui.LabelText("Value", elementType.Name);
        return false;
    }

    private static bool CanCreateListItem(object owner, PropertyInfo prop, Type elementType)
    {
        var listFactory = prop.GetCustomAttribute<InspectorListFactoryAttribute>();
        if (listFactory != null)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var method = owner.GetType().GetMethod(listFactory.MethodName, flags, binder: null, types: Type.EmptyTypes, modifiers: null);
            return method != null && elementType.IsAssignableFrom(method.ReturnType);
        }

        if (elementType == typeof(string))
            return true;
        if (elementType.IsValueType)
            return true;

        return elementType.GetConstructor(Type.EmptyTypes) != null;
    }

    private static bool TryCreateListItem(object owner, PropertyInfo prop, Type elementType, out object? value)
    {
        value = null;
        var listFactory = prop.GetCustomAttribute<InspectorListFactoryAttribute>();
        if (listFactory != null && TryInvokeFactory(owner, listFactory.MethodName, elementType, out value))
            return true;

        if (elementType == typeof(string))
        {
            value = string.Empty;
            return true;
        }

        if (elementType.IsValueType)
        {
            value = Activator.CreateInstance(elementType);
            return value != null;
        }

        var ctor = elementType.GetConstructor(Type.EmptyTypes);
        if (ctor == null)
            return false;

        value = Activator.CreateInstance(elementType);
        return value != null;
    }

    private static bool TryInvokeFactory(object owner, string methodName, Type elementType, out object? value)
    {
        value = null;
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        var method = owner.GetType().GetMethod(methodName, flags, binder: null, types: Type.EmptyTypes, modifiers: null);
        if (method == null)
            return false;

        try
        {
            var result = method.Invoke(owner, null);
            if (result == null || !elementType.IsAssignableFrom(result.GetType()))
                return false;

            value = result;
            return true;
        }
        catch (Exception ex)
        {
            FrinkyLog.Error($"InspectorListFactory '{methodName}' failed on {owner.GetType().Name}: {ex.InnerException?.Message ?? ex.Message}");
            return false;
        }
    }

    private static bool TryCreateInlineObject(Type objectType, out object? value)
    {
        value = null;
        if (objectType.IsValueType)
        {
            value = Activator.CreateInstance(objectType);
            return value != null;
        }

        var ctor = objectType.GetConstructor(Type.EmptyTypes);
        if (ctor == null)
            return false;

        value = Activator.CreateInstance(objectType);
        return value != null;
    }

    private sealed class ListElementPropertyBridge(System.Collections.IList list, int index)
    {
        public static readonly PropertyInfo ValueProperty = typeof(ListElementPropertyBridge)
            .GetProperty(nameof(Value), BindingFlags.Public | BindingFlags.Instance)!;

        public EntityReference Value
        {
            get => list[index] is EntityReference entityReference ? entityReference : EntityReference.None;
            set => list[index] = value;
        }
    }

    private static void DisposeIfNeeded(object? value)
    {
        if (value is not IDisposable disposable)
            return;

        try { disposable.Dispose(); }
        catch { }
    }

    // ─── Reusable helpers ───────────────────────────────────────────────

    private static readonly Vector4 AccentBarColor = new(0.45f, 0.45f, 0.45f, 0.25f);

    private static void DrawIndentedWithAccentBar(Action drawContent)
    {
        const float indent = 12f;
        float treeIndent = ImGui.GetStyle().IndentSpacing;
        float reduce = treeIndent - indent;
        ImGui.Unindent(reduce);

        var startPos = ImGui.GetCursorScreenPos();
        float startY = startPos.Y;

        drawContent();

        float endY = ImGui.GetCursorScreenPos().Y;
        ImGui.Indent(reduce);

        if (endY > startY)
        {
            var drawList = ImGui.GetWindowDrawList();
            float barX = startPos.X - indent + 4f;
            drawList.AddRectFilled(
                new Vector2(barX, startY),
                new Vector2(barX + 2f, endY),
                ImGui.GetColorU32(AccentBarColor));
        }
    }

    /// <summary>
    /// Draws move-up, move-down, and remove buttons for a list item.
    /// Returns (moveUp, moveDown, remove) indices if any button was clicked.
    /// </summary>
    private static (int? moveUp, int? moveDown, int? remove) DrawListItemButtons(int index, int count)
    {
        int? moveUp = null, moveDown = null, remove = null;

        float buttonSize = ImGui.GetFrameHeight();
        float totalButtonWidth = buttonSize * 3f + ImGui.GetStyle().ItemSpacing.X * 2f;
        ImGui.SameLine(ImGui.GetContentRegionAvail().X - totalButtonWidth + ImGui.GetCursorPosX());

        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0, 0, 0, 0));

        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ImGui.GetStyle().Colors[(int)ImGuiCol.HeaderHovered]);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, ImGui.GetStyle().Colors[(int)ImGuiCol.HeaderActive]);

        ImGui.BeginDisabled(index <= 0);
        if (ImGui.ArrowButton("##up", ImGuiDir.Up)) moveUp = index;
        ImGui.EndDisabled();
        ImGui.SameLine();

        ImGui.BeginDisabled(index >= count - 1);
        if (ImGui.ArrowButton("##down", ImGuiDir.Down)) moveDown = index;
        ImGui.EndDisabled();
        ImGui.SameLine();

        ImGui.PopStyleColor(2); // swap header hover/active for red hover/active
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.8f, 0.2f, 0.2f, 0.4f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.8f, 0.2f, 0.2f, 0.6f));
        if (ImGui.Button("X", new Vector2(buttonSize, buttonSize))) remove = index;
        ImGui.PopStyleColor(3); // red active, red hovered, transparent button

        return (moveUp, moveDown, remove);
    }

    private static readonly Vector4 WarningColor = new(1f, 0.55f, 0.25f, 1f);
    private static readonly Vector4 ErrorColor = new(1f, 0.3f, 0.3f, 1f);

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
