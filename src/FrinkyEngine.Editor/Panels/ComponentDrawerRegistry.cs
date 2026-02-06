using System.Numerics;
using System.Reflection;
using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.Components;
using FrinkyEngine.Core.ECS;
using ImGuiNET;
using Raylib_cs;

namespace FrinkyEngine.Editor.Panels;

public static class ComponentDrawerRegistry
{
    private static readonly Dictionary<Type, Action<Component>> _drawers = new();

    static ComponentDrawerRegistry()
    {
        Register<TransformComponent>(DrawTransform);
        Register<CameraComponent>(DrawCamera);
        Register<LightComponent>(DrawLight);
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
        ImGui.PushID(label);

        ImGui.Columns(2, null, false);
        ImGui.SetColumnWidth(0, 80);
        ImGui.Text(label);
        ImGui.NextColumn();

        float lineHeight = ImGui.GetFrameHeight();
        var buttonSize = new Vector2(lineHeight + 3f, lineHeight);
        float availWidth = ImGui.GetContentRegionAvail().X;
        float fieldWidth = (availWidth - 3f * buttonSize.X - 2f * ImGui.GetStyle().ItemSpacing.X) / 3f;

        // X
        ImGui.PushStyleColor(ImGuiCol.Button, ColorRed);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ColorRedHover);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, ColorRed);
        if (ImGui.Button("X", buttonSize))
        {
            value.X = resetValue;
            changed = true;
        }
        ImGui.PopStyleColor(3);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(fieldWidth);
        if (ImGui.DragFloat("##X", ref value.X, speed))
            changed = true;

        ImGui.SameLine();

        // Y
        ImGui.PushStyleColor(ImGuiCol.Button, ColorGreen);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ColorGreenHover);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, ColorGreen);
        if (ImGui.Button("Y", buttonSize))
        {
            value.Y = resetValue;
            changed = true;
        }
        ImGui.PopStyleColor(3);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(fieldWidth);
        if (ImGui.DragFloat("##Y", ref value.Y, speed))
            changed = true;

        ImGui.SameLine();

        // Z
        ImGui.PushStyleColor(ImGuiCol.Button, ColorBlue);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ColorBlueHover);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, ColorBlue);
        if (ImGui.Button("Z", buttonSize))
        {
            value.Z = resetValue;
            changed = true;
        }
        ImGui.PopStyleColor(3);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(fieldWidth);
        if (ImGui.DragFloat("##Z", ref value.Z, speed))
            changed = true;

        ImGui.Columns(1);
        ImGui.PopID();
        return changed;
    }

    private static void DrawCamera(Component c)
    {
        var cam = (CameraComponent)c;

        float fov = cam.FieldOfView;
        if (ImGui.DragFloat("Field of View", ref fov, 0.5f, 1f, 179f))
            cam.FieldOfView = fov;

        float near = cam.NearPlane;
        if (ImGui.DragFloat("Near Plane", ref near, 0.01f, 0.001f, 100f))
            cam.NearPlane = near;

        float far = cam.FarPlane;
        if (ImGui.DragFloat("Far Plane", ref far, 1f, 1f, 10000f))
            cam.FarPlane = far;

        var projType = (int)cam.Projection;
        if (ImGui.Combo("Projection", ref projType, "Perspective\0Orthographic\0"))
            cam.Projection = (ProjectionType)projType;

        var clearColor = ColorToVec4(cam.ClearColor);
        if (ImGui.ColorEdit4("Clear Color", ref clearColor))
            cam.ClearColor = Vec4ToColor(clearColor);

        bool isMain = cam.IsMain;
        if (ImGui.Checkbox("Is Main", ref isMain))
            cam.IsMain = isMain;
    }

    private static void DrawLight(Component c)
    {
        var light = (LightComponent)c;

        var lightType = (int)light.LightType;
        if (ImGui.Combo("Type", ref lightType, "Directional\0Point\0Skylight\0"))
            light.LightType = (LightType)lightType;

        var color = ColorToVec4(light.LightColor);
        if (ImGui.ColorEdit4("Color", ref color))
            light.LightColor = Vec4ToColor(color);

        float intensity = light.Intensity;
        if (ImGui.DragFloat("Intensity", ref intensity, 0.05f, 0f, 10f))
            light.Intensity = intensity;

        if (light.LightType == LightType.Point)
        {
            float range = light.Range;
            if (ImGui.DragFloat("Range", ref range, 0.1f, 0f, 100f))
                light.Range = range;
        }
    }

    private static void DrawMeshRenderer(Component c)
    {
        var mr = (MeshRendererComponent)c;

        // Model Path with browse button on its own row
        ImGui.Text("Model Path");
        ImGui.SetNextItemWidth(-30);
        string modelPath = mr.ModelPath;
        if (ImGui.InputText("##ModelPath", ref modelPath, 256))
            mr.ModelPath = modelPath;
        ImGui.SameLine();
        if (ImGui.Button("...##BrowseModel"))
            ImGui.OpenPopup("ModelBrowser");

        if (ImGui.BeginPopup("ModelBrowser"))
        {
            var models = AssetDatabase.Instance.GetAssets(AssetType.Model);
            if (models.Count == 0)
            {
                ImGui.TextDisabled("No models found");
            }
            else
            {
                foreach (var asset in models)
                {
                    if (ImGui.Selectable(asset.RelativePath))
                    {
                        mr.ModelPath = asset.RelativePath;
                    }
                }
            }
            ImGui.EndPopup();
        }

        ImGui.Spacing();

        var tint = ColorToVec4(mr.Tint);
        if (ImGui.ColorEdit4("Tint", ref tint))
            mr.Tint = Vec4ToColor(tint);
    }

    private static void DrawPrimitive(Component c)
    {
        var prim = (PrimitiveComponent)c;

        var matType = (int)prim.MaterialType;
        if (ImGui.Combo("Material Type", ref matType, "SolidColor\0Textured\0"))
            prim.MaterialType = (FrinkyEngine.Core.Rendering.MaterialType)matType;

        var tint = ColorToVec4(prim.Tint);
        if (ImGui.ColorEdit4("Tint", ref tint))
            prim.Tint = Vec4ToColor(tint);

        if (prim.MaterialType == FrinkyEngine.Core.Rendering.MaterialType.Textured)
        {
            ImGui.Text("Texture Path");
            ImGui.SetNextItemWidth(-30);
            string texPath = prim.TexturePath;
            if (ImGui.InputText("##TexturePath", ref texPath, 256))
                prim.TexturePath = texPath;
            ImGui.SameLine();
            if (ImGui.Button("...##BrowseTexture"))
                ImGui.OpenPopup("TextureBrowser");

            if (ImGui.BeginPopup("TextureBrowser"))
            {
                var textures = AssetDatabase.Instance.GetAssets(AssetType.Texture);
                if (textures.Count == 0)
                {
                    ImGui.TextDisabled("No textures found");
                }
                else
                {
                    foreach (var asset in textures)
                    {
                        if (ImGui.Selectable(asset.RelativePath))
                        {
                            prim.TexturePath = asset.RelativePath;
                        }
                    }
                }
                ImGui.EndPopup();
            }
        }

        ImGui.Separator();
        ImGui.Text("Shape Properties");
        ImGui.Spacing();

        DrawSubclassProperties(c);
    }

    private static void DrawSubclassProperties(Component component)
    {
        var type = component.GetType();
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            if (!prop.CanRead || !prop.CanWrite) continue;
            DrawProperty(component, prop);
        }
    }

    public static void DrawReflection(Component component)
    {
        var type = component.GetType();
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanRead || !prop.CanWrite) continue;
            if (prop.Name is "Entity" or "HasStarted" or "Enabled") continue;
            if (prop.Name is "LoadedModel" or "GeneratedModel") continue;

            DrawProperty(component, prop);
        }
    }

    private static void DrawProperty(Component component, PropertyInfo prop)
    {
        var propType = prop.PropertyType;
        var label = prop.Name;

        if (propType == typeof(float))
        {
            float val = (float)prop.GetValue(component)!;
            if (ImGui.DragFloat(label, ref val, 0.1f))
                prop.SetValue(component, val);
        }
        else if (propType == typeof(int))
        {
            int val = (int)prop.GetValue(component)!;
            if (ImGui.DragInt(label, ref val))
                prop.SetValue(component, val);
        }
        else if (propType == typeof(bool))
        {
            bool val = (bool)prop.GetValue(component)!;
            if (ImGui.Checkbox(label, ref val))
                prop.SetValue(component, val);
        }
        else if (propType == typeof(string))
        {
            string val = (string)(prop.GetValue(component) ?? "");
            if (ImGui.InputText(label, ref val, 256))
                prop.SetValue(component, val);
        }
        else if (propType == typeof(Vector3))
        {
            var val = (Vector3)prop.GetValue(component)!;
            if (ImGui.DragFloat3(label, ref val, 0.1f))
                prop.SetValue(component, val);
        }
        else if (propType == typeof(Vector2))
        {
            var val = (Vector2)prop.GetValue(component)!;
            if (ImGui.DragFloat2(label, ref val, 0.1f))
                prop.SetValue(component, val);
        }
        else if (propType == typeof(Quaternion))
        {
            var q = (Quaternion)prop.GetValue(component)!;
            var euler = Core.FrinkyMath.QuaternionToEuler(q);
            if (ImGui.DragFloat3(label, ref euler, 0.5f))
                prop.SetValue(component, Core.FrinkyMath.EulerToQuaternion(euler));
        }
        else if (propType == typeof(Color))
        {
            var color = ColorToVec4((Color)prop.GetValue(component)!);
            if (ImGui.ColorEdit4(label, ref color))
                prop.SetValue(component, Vec4ToColor(color));
        }
        else if (propType.IsEnum)
        {
            var val = (int)prop.GetValue(component)!;
            var names = Enum.GetNames(propType);
            if (ImGui.Combo(label, ref val, names, names.Length))
                prop.SetValue(component, Enum.ToObject(propType, val));
        }
        else
        {
            ImGui.LabelText(label, propType.Name);
        }
    }

    private static Vector4 ColorToVec4(Color c) =>
        new(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);

    private static Color Vec4ToColor(Vector4 v) =>
        new((byte)(v.X * 255), (byte)(v.Y * 255), (byte)(v.Z * 255), (byte)(v.W * 255));
}
