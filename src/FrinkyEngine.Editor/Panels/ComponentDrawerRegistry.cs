using System.Numerics;
using System.Reflection;
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
    }

    public static void Register<T>(Action<Component> drawer) where T : Component
    {
        _drawers[typeof(T)] = drawer;
    }

    public static bool Draw(Component component)
    {
        if (_drawers.TryGetValue(component.GetType(), out var drawer))
        {
            drawer(component);
            return true;
        }
        return false;
    }

    private static void DrawTransform(Component c)
    {
        var t = (TransformComponent)c;
        var pos = t.LocalPosition;
        if (ImGui.DragFloat3("Position", ref pos, 0.1f))
            t.LocalPosition = pos;

        var euler = t.EulerAngles;
        if (ImGui.DragFloat3("Rotation", ref euler, 0.5f))
            t.EulerAngles = euler;

        var scale = t.LocalScale;
        if (ImGui.DragFloat3("Scale", ref scale, 0.05f))
            t.LocalScale = scale;
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
        if (ImGui.Combo("Type", ref lightType, "Directional\0Point\0"))
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

        string modelPath = mr.ModelPath;
        if (ImGui.InputText("Model Path", ref modelPath, 256))
            mr.ModelPath = modelPath;

        string matPath = mr.MaterialPath;
        if (ImGui.InputText("Material Path", ref matPath, 256))
            mr.MaterialPath = matPath;

        var tint = ColorToVec4(mr.Tint);
        if (ImGui.ColorEdit4("Tint", ref tint))
            mr.Tint = Vec4ToColor(tint);
    }

    public static void DrawReflection(Component component)
    {
        var type = component.GetType();
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanRead || !prop.CanWrite) continue;
            if (prop.Name is "Entity" or "HasStarted" or "Enabled") continue;
            if (prop.Name == "LoadedModel") continue;

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
