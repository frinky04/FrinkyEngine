using System.Numerics;
using System.Reflection;
using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.Components;
using FrinkyEngine.Core.ECS;
using ImGuiNET;
using Raylib_cs;
using Texture2D = Raylib_cs.Texture2D;

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
        Register<RigidbodyComponent>(DrawRigidbody);
        Register<BoxColliderComponent>(DrawBoxCollider);
        Register<SphereColliderComponent>(DrawSphereCollider);
        Register<CapsuleColliderComponent>(DrawCapsuleCollider);
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

        ImGui.Columns(2, null, false);
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

    private static void DrawCamera(Component c)
    {
        var cam = (CameraComponent)c;
        var app = EditorApplication.Instance;

        float fov = cam.FieldOfView;
        if (ImGui.DragFloat("Field of View", ref fov, 0.5f, 1f, 179f))
            cam.FieldOfView = fov;
        TrackContinuousUndo(app);

        float near = cam.NearPlane;
        if (ImGui.DragFloat("Near Plane", ref near, 0.01f, 0.001f, 100f))
            cam.NearPlane = near;
        TrackContinuousUndo(app);

        float far = cam.FarPlane;
        if (ImGui.DragFloat("Far Plane", ref far, 1f, 1f, 10000f))
            cam.FarPlane = far;
        TrackContinuousUndo(app);

        var projType = (int)cam.Projection;
        if (ImGui.Combo("Projection", ref projType, "Perspective\0Orthographic\0"))
        {
            app.RecordUndo();
            cam.Projection = (ProjectionType)projType;
            app.RefreshUndoBaseline();
        }

        var clearColor = ColorToVec4(cam.ClearColor);
        if (ImGui.ColorEdit4("Clear Color", ref clearColor))
            cam.ClearColor = Vec4ToColor(clearColor);
        TrackContinuousUndo(app);

        bool isMain = cam.IsMain;
        if (ImGui.Checkbox("Is Main", ref isMain))
        {
            app.RecordUndo();
            cam.IsMain = isMain;
            app.RefreshUndoBaseline();
        }
    }

    private static void DrawLight(Component c)
    {
        var light = (LightComponent)c;
        var app = EditorApplication.Instance;

        var lightType = (int)light.LightType;
        if (ImGui.Combo("Type", ref lightType, "Directional\0Point\0Skylight\0"))
        {
            app.RecordUndo();
            light.LightType = (LightType)lightType;
            app.RefreshUndoBaseline();
        }

        var color = ColorToVec4(light.LightColor);
        if (ImGui.ColorEdit4("Color", ref color))
            light.LightColor = Vec4ToColor(color);
        TrackContinuousUndo(app);

        float intensity = light.Intensity;
        if (ImGui.DragFloat("Intensity", ref intensity, 0.05f, 0f, 10f))
            light.Intensity = intensity;
        TrackContinuousUndo(app);

        if (light.LightType == LightType.Point)
        {
            float range = light.Range;
            if (ImGui.DragFloat("Range", ref range, 0.1f, 0f, 100f))
                light.Range = range;
            TrackContinuousUndo(app);
        }
    }

    private static void DrawMeshRenderer(Component c)
    {
        var mr = (MeshRendererComponent)c;
        var app = EditorApplication.Instance;

        // Model Path with browse button on its own row
        ImGui.Text("Model Path");
        ImGui.SetNextItemWidth(-30);
        string modelPath = mr.ModelPath;
        if (ImGui.InputText("##ModelPath", ref modelPath, 256))
            mr.ModelPath = modelPath;
        TrackContinuousUndo(app);
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
                    ImGui.PushID(asset.RelativePath);
                    if (AssetSelectable(AssetType.Model, asset.RelativePath))
                    {
                        app.RecordUndo();
                        mr.ModelPath = asset.RelativePath;
                        app.RefreshUndoBaseline();
                    }
                    ImGui.PopID();
                }
            }
            ImGui.EndPopup();
        }

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
                    var matType = (int)slot.MaterialType;
                    if (ImGui.Combo("Type", ref matType, "SolidColor\0Textured\0TriplanarTexture\0"))
                    {
                        app.RecordUndo();
                        slot.MaterialType = (Core.Rendering.MaterialType)matType;
                        slotsChanged = true;
                        app.RefreshUndoBaseline();
                    }

                    if (slot.MaterialType is Core.Rendering.MaterialType.Textured or Core.Rendering.MaterialType.TriplanarTexture)
                    {
                        ImGui.Text("Texture");
                        ImGui.SetNextItemWidth(-30);
                        string texPath = slot.TexturePath;
                        if (ImGui.InputText("##SlotTexture", ref texPath, 256))
                        {
                            slot.TexturePath = texPath;
                            slotsChanged = true;
                        }
                        TrackContinuousUndo(app);
                        ImGui.SameLine();
                        if (ImGui.Button("...##BrowseSlotTex"))
                            ImGui.OpenPopup("SlotTexBrowser");

                        if (ImGui.BeginPopup("SlotTexBrowser"))
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
                                    ImGui.PushID(asset.RelativePath);
                                    if (AssetSelectable(AssetType.Texture, asset.RelativePath))
                                    {
                                        app.RecordUndo();
                                        slot.TexturePath = asset.RelativePath;
                                        slotsChanged = true;
                                        app.RefreshUndoBaseline();
                                    }
                                    ImGui.PopID();
                                }
                            }
                            ImGui.EndPopup();
                        }
                    }

                    if (slot.MaterialType == Core.Rendering.MaterialType.TriplanarTexture)
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

        var matType = (int)prim.MaterialType;
        if (ImGui.Combo("Material Type", ref matType, "SolidColor\0Textured\0TriplanarTexture\0"))
        {
            app.RecordUndo();
            prim.MaterialType = (FrinkyEngine.Core.Rendering.MaterialType)matType;
            app.RefreshUndoBaseline();
        }

        var tint = ColorToVec4(prim.Tint);
        if (ImGui.ColorEdit4("Tint", ref tint))
            prim.Tint = Vec4ToColor(tint);
        TrackContinuousUndo(app);

        if (prim.MaterialType is FrinkyEngine.Core.Rendering.MaterialType.Textured or FrinkyEngine.Core.Rendering.MaterialType.TriplanarTexture)
        {
            ImGui.Text("Texture Path");
            ImGui.SetNextItemWidth(-30);
            string texPath = prim.TexturePath;
            if (ImGui.InputText("##TexturePath", ref texPath, 256))
                prim.TexturePath = texPath;
            TrackContinuousUndo(app);
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
                        ImGui.PushID(asset.RelativePath);
                        if (AssetSelectable(AssetType.Texture, asset.RelativePath))
                        {
                            app.RecordUndo();
                            prim.TexturePath = asset.RelativePath;
                            app.RefreshUndoBaseline();
                        }
                        ImGui.PopID();
                    }
                }
                ImGui.EndPopup();
            }
        }

        if (prim.MaterialType == FrinkyEngine.Core.Rendering.MaterialType.TriplanarTexture)
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

    private static void DrawRigidbody(Component c)
    {
        var rb = (RigidbodyComponent)c;
        var app = EditorApplication.Instance;

        int motion = (int)rb.MotionType;
        if (ImGui.Combo("Motion Type", ref motion, "Dynamic\0Kinematic\0Static\0"))
        {
            app.RecordUndo();
            rb.MotionType = (BodyMotionType)motion;
            app.RefreshUndoBaseline();
        }

        if (rb.MotionType == BodyMotionType.Dynamic)
        {
            float mass = rb.Mass;
            if (ImGui.DragFloat("Mass", ref mass, 0.05f, 0.0001f, 100000f))
                rb.Mass = mass;
            TrackContinuousUndo(app);
        }
        else
        {
            ImGui.TextDisabled("Mass is used for Dynamic bodies only.");
        }

        float linearDamping = rb.LinearDamping;
        if (ImGui.DragFloat("Linear Damping", ref linearDamping, 0.005f, 0f, 1f))
            rb.LinearDamping = linearDamping;
        TrackContinuousUndo(app);

        float angularDamping = rb.AngularDamping;
        if (ImGui.DragFloat("Angular Damping", ref angularDamping, 0.005f, 0f, 1f))
            rb.AngularDamping = angularDamping;
        TrackContinuousUndo(app);

        bool continuousDetection = rb.ContinuousDetection;
        if (ImGui.Checkbox("Continuous Detection", ref continuousDetection))
        {
            app.RecordUndo();
            rb.ContinuousDetection = continuousDetection;
            app.RefreshUndoBaseline();
        }

        ImGui.Separator();
        ImGui.Text("Axis Locks");

        bool lockPositionX = rb.LockPositionX;
        if (ImGui.Checkbox("Lock Position X", ref lockPositionX))
        {
            app.RecordUndo();
            rb.LockPositionX = lockPositionX;
            app.RefreshUndoBaseline();
        }

        bool lockPositionY = rb.LockPositionY;
        if (ImGui.Checkbox("Lock Position Y", ref lockPositionY))
        {
            app.RecordUndo();
            rb.LockPositionY = lockPositionY;
            app.RefreshUndoBaseline();
        }

        bool lockPositionZ = rb.LockPositionZ;
        if (ImGui.Checkbox("Lock Position Z", ref lockPositionZ))
        {
            app.RecordUndo();
            rb.LockPositionZ = lockPositionZ;
            app.RefreshUndoBaseline();
        }

        bool lockRotationX = rb.LockRotationX;
        if (ImGui.Checkbox("Lock Rotation X", ref lockRotationX))
        {
            app.RecordUndo();
            rb.LockRotationX = lockRotationX;
            app.RefreshUndoBaseline();
        }

        bool lockRotationY = rb.LockRotationY;
        if (ImGui.Checkbox("Lock Rotation Y", ref lockRotationY))
        {
            app.RecordUndo();
            rb.LockRotationY = lockRotationY;
            app.RefreshUndoBaseline();
        }

        bool lockRotationZ = rb.LockRotationZ;
        if (ImGui.Checkbox("Lock Rotation Z", ref lockRotationZ))
        {
            app.RecordUndo();
            rb.LockRotationZ = lockRotationZ;
            app.RefreshUndoBaseline();
        }

        bool hasCollider = rb.Entity.Components.Any(component => component is ColliderComponent collider && collider.Enabled);
        if (!hasCollider)
        {
            ImGui.Spacing();
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1f, 0.55f, 0.25f, 1f));
            ImGui.TextWrapped("Warning: Rigidbody is ignored until an enabled collider component is added.");
            ImGui.PopStyleColor();
        }

        if (rb.Entity.Transform.Parent != null)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1f, 0.55f, 0.25f, 1f));
            ImGui.TextWrapped("Warning: Parented rigidbodies are not simulated.");
            ImGui.PopStyleColor();
        }
    }

    private static void DrawBoxCollider(Component c)
    {
        var collider = (BoxColliderComponent)c;
        DrawColliderCommon(collider);

        var size = collider.Size;
        if (ImGui.DragFloat3("Size", ref size, 0.05f, 0.001f, 10000f))
            collider.Size = size;
        TrackContinuousUndo(EditorApplication.Instance);
    }

    private static void DrawSphereCollider(Component c)
    {
        var collider = (SphereColliderComponent)c;
        DrawColliderCommon(collider);

        float radius = collider.Radius;
        if (ImGui.DragFloat("Radius", ref radius, 0.05f, 0.001f, 10000f))
            collider.Radius = radius;
        TrackContinuousUndo(EditorApplication.Instance);
    }

    private static void DrawCapsuleCollider(Component c)
    {
        var collider = (CapsuleColliderComponent)c;
        DrawColliderCommon(collider);

        float radius = collider.Radius;
        if (ImGui.DragFloat("Radius", ref radius, 0.05f, 0.001f, 10000f))
            collider.Radius = radius;
        TrackContinuousUndo(EditorApplication.Instance);

        float length = collider.Length;
        if (ImGui.DragFloat("Length", ref length, 0.05f, 0.001f, 10000f))
            collider.Length = length;
        TrackContinuousUndo(EditorApplication.Instance);
    }

    private static void DrawColliderCommon(ColliderComponent collider)
    {
        var app = EditorApplication.Instance;

        float friction = collider.Friction;
        if (ImGui.DragFloat("Friction", ref friction, 0.01f, 0f, 10f))
            collider.Friction = friction;
        TrackContinuousUndo(app);

        float restitution = collider.Restitution;
        if (ImGui.DragFloat("Restitution", ref restitution, 0.01f, 0f, 1f))
            collider.Restitution = restitution;
        TrackContinuousUndo(app);

        var center = collider.Center;
        if (ImGui.DragFloat3("Center", ref center, 0.05f))
            collider.Center = center;
        TrackContinuousUndo(app);

        int colliderCount = collider.Entity.Components.Count(component => component is ColliderComponent enabledCollider && enabledCollider.Enabled);
        if (colliderCount > 1)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1f, 0.55f, 0.25f, 1f));
            ImGui.TextWrapped("Warning: Multiple enabled colliders are present. Only the first enabled collider is used.");
            ImGui.PopStyleColor();
        }
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
            if (prop.Name is "RenderModel") continue;

            DrawProperty(component, prop);
        }
    }

    private static void DrawProperty(Component component, PropertyInfo prop)
    {
        var propType = prop.PropertyType;
        var label = prop.Name;
        var app = EditorApplication.Instance;

        if (propType == typeof(float))
        {
            float val = (float)prop.GetValue(component)!;
            if (ImGui.DragFloat(label, ref val, 0.1f))
                prop.SetValue(component, val);
            TrackContinuousUndo(app);
        }
        else if (propType == typeof(int))
        {
            int val = (int)prop.GetValue(component)!;
            if (ImGui.DragInt(label, ref val))
                prop.SetValue(component, val);
            TrackContinuousUndo(app);
        }
        else if (propType == typeof(bool))
        {
            bool val = (bool)prop.GetValue(component)!;
            if (ImGui.Checkbox(label, ref val))
            {
                app.RecordUndo();
                prop.SetValue(component, val);
                app.RefreshUndoBaseline();
            }
        }
        else if (propType == typeof(string))
        {
            string val = (string)(prop.GetValue(component) ?? "");
            if (ImGui.InputText(label, ref val, 256))
                prop.SetValue(component, val);
            TrackContinuousUndo(app);
        }
        else if (propType == typeof(Vector3))
        {
            var val = (Vector3)prop.GetValue(component)!;
            if (ImGui.DragFloat3(label, ref val, 0.1f))
                prop.SetValue(component, val);
            TrackContinuousUndo(app);
        }
        else if (propType == typeof(Vector2))
        {
            var val = (Vector2)prop.GetValue(component)!;
            if (ImGui.DragFloat2(label, ref val, 0.1f))
                prop.SetValue(component, val);
            TrackContinuousUndo(app);
        }
        else if (propType == typeof(Quaternion))
        {
            var q = (Quaternion)prop.GetValue(component)!;
            var euler = Core.FrinkyMath.QuaternionToEuler(q);
            if (ImGui.DragFloat3(label, ref euler, 0.5f))
                prop.SetValue(component, Core.FrinkyMath.EulerToQuaternion(euler));
            TrackContinuousUndo(app);
        }
        else if (propType == typeof(Color))
        {
            var color = ColorToVec4((Color)prop.GetValue(component)!);
            if (ImGui.ColorEdit4(label, ref color))
                prop.SetValue(component, Vec4ToColor(color));
            TrackContinuousUndo(app);
        }
        else if (propType.IsEnum)
        {
            var val = (int)prop.GetValue(component)!;
            var names = Enum.GetNames(propType);
            if (ImGui.Combo(label, ref val, names, names.Length))
            {
                app.RecordUndo();
                prop.SetValue(component, Enum.ToObject(propType, val));
                app.RefreshUndoBaseline();
            }
        }
        else
        {
            ImGui.LabelText(label, propType.Name);
        }
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

    private static float DrawAssetIcon(AssetType type)
    {
        float size = EditorIcons.GetIconSize();
        var icon = EditorIcons.GetIcon(type);
        if (icon is Texture2D tex)
        {
            ImGui.Image((nint)tex.Id, new Vector2(size, size));
            ImGui.SameLine(0, 4);
        }
        return size;
    }

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
}
