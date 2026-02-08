using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.Components;
using FrinkyEngine.Core.ECS;
using FrinkyEngine.Core.Scene;
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
        Register<CharacterControllerComponent>(DrawCharacterController);
        Register<SimplePlayerInputComponent>(DrawSimplePlayerInput);
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

        float fov = cam.FieldOfView;
        DrawDragFloat("Field of View", ref fov, 0.5f, 1f, 179f);
        cam.FieldOfView = fov;

        float near = cam.NearPlane;
        DrawDragFloat("Near Plane", ref near, 0.01f, 0.001f, 100f);
        cam.NearPlane = near;

        float far = cam.FarPlane;
        DrawDragFloat("Far Plane", ref far, 1f, 1f, 10000f);
        cam.FarPlane = far;

        DrawEnumCombo("Projection", cam.Projection, v => cam.Projection = v);
        DrawColorEdit4("Clear Color", cam.ClearColor, v => cam.ClearColor = v);
        DrawCheckbox("Is Main", cam.IsMain, v => cam.IsMain = v);
    }

    private static void DrawLight(Component c)
    {
        var light = (LightComponent)c;

        DrawEnumCombo("Type", light.LightType, v => light.LightType = v);
        DrawColorEdit4("Color", light.LightColor, v => light.LightColor = v);

        float intensity = light.Intensity;
        DrawDragFloat("Intensity", ref intensity, 0.05f, 0f, 10f);
        light.Intensity = intensity;

        if (light.LightType == LightType.Point)
        {
            float range = light.Range;
            DrawDragFloat("Range", ref range, 0.1f, 0f, 100f);
            light.Range = range;
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

        DrawEnumCombo("Motion Type", rb.MotionType, v => rb.MotionType = v);

        if (rb.MotionType == BodyMotionType.Dynamic)
        {
            float mass = rb.Mass;
            DrawDragFloat("Mass", ref mass, 0.05f, 0.0001f, 100000f);
            rb.Mass = mass;
        }
        else
        {
            ImGui.TextDisabled("Mass is used for Dynamic bodies only.");
        }

        float linearDamping = rb.LinearDamping;
        DrawDragFloat("Linear Damping", ref linearDamping, 0.005f, 0f, 1f);
        rb.LinearDamping = linearDamping;

        float angularDamping = rb.AngularDamping;
        DrawDragFloat("Angular Damping", ref angularDamping, 0.005f, 0f, 1f);
        rb.AngularDamping = angularDamping;

        DrawCheckbox("Continuous Detection", rb.ContinuousDetection, v => rb.ContinuousDetection = v);
        DrawEnumCombo("Interpolation", rb.InterpolationMode, v => rb.InterpolationMode = v);

        DrawSection("Axis Locks");
        DrawCheckbox("Lock Position X", rb.LockPositionX, v => rb.LockPositionX = v);
        DrawCheckbox("Lock Position Y", rb.LockPositionY, v => rb.LockPositionY = v);
        DrawCheckbox("Lock Position Z", rb.LockPositionZ, v => rb.LockPositionZ = v);
        DrawCheckbox("Lock Rotation X", rb.LockRotationX, v => rb.LockRotationX = v);
        DrawCheckbox("Lock Rotation Y", rb.LockRotationY, v => rb.LockRotationY = v);
        DrawCheckbox("Lock Rotation Z", rb.LockRotationZ, v => rb.LockRotationZ = v);

        bool hasCollider = rb.Entity.Components.Any(component => component is ColliderComponent collider && collider.Enabled);
        if (!hasCollider)
        {
            ImGui.Spacing();
            DrawWarning("Rigidbody is ignored until an enabled collider component is added.");
        }

        if (rb.Entity.Transform.Parent != null)
            DrawWarning("Parented rigidbodies are not simulated.");
    }

    private static void DrawCharacterController(Component c)
    {
        var controller = (CharacterControllerComponent)c;

        // ── Movement ──
        DrawSection("Movement");
        float moveSpeed = controller.MoveSpeed;
        DrawDragFloat("Move Speed", ref moveSpeed, 0.05f, 0f, 1000f);
        controller.MoveSpeed = moveSpeed;

        float jumpVelocity = controller.JumpVelocity;
        DrawDragFloat("Jump Velocity", ref jumpVelocity, 0.05f, 0f, 1000f);
        controller.JumpVelocity = jumpVelocity;

        float maxSlope = controller.MaxSlopeDegrees;
        DrawDragFloat("Max Slope (deg)", ref maxSlope, 0.25f, 0f, 89f);
        controller.MaxSlopeDegrees = maxSlope;

        // ── Forces ──
        DrawSection("Forces");
        float maxHForce = controller.MaximumHorizontalForce;
        DrawDragFloat("Max Horizontal Force", ref maxHForce, 0.25f, 0f, 100000f);
        controller.MaximumHorizontalForce = maxHForce;

        float maxVForce = controller.MaximumVerticalForce;
        DrawDragFloat("Max Vertical Force", ref maxVForce, 0.25f, 0f, 100000f);
        controller.MaximumVerticalForce = maxVForce;

        // ── Air Control ──
        DrawSection("Air Control");
        float airForce = controller.AirControlForceScale;
        DrawDragFloat("Force Scale##air", ref airForce, 0.01f, 0f, 10f);
        controller.AirControlForceScale = airForce;

        float airSpeed = controller.AirControlSpeedScale;
        DrawDragFloat("Speed Scale##air", ref airSpeed, 0.01f, 0f, 10f);
        controller.AirControlSpeedScale = airSpeed;

        // ── View Direction ──
        DrawSection("View Direction");
        DrawCheckbox("Use Entity Forward", controller.UseEntityForwardAsViewDirection,
            v => controller.UseEntityForwardAsViewDirection = v);

        if (!controller.UseEntityForwardAsViewDirection)
        {
            var viewDir = controller.ViewDirectionOverride;
            if (DrawColoredVector3("View Override", ref viewDir, 0.05f))
                controller.ViewDirectionOverride = viewDir;
        }

        // ── Debug Info ──
        DrawSection("Debug Info");
        DrawReadOnlyText("Supported", controller.Supported ? "Yes" : "No");
        var target = controller.LastComputedTargetVelocity;
        DrawReadOnlyText("Target Velocity", $"{target.X:0.00}, {target.Y:0.00}, {target.Z:0.00}");

        // ── Warnings ──
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
    }

    private static void DrawSimplePlayerInput(Component c)
    {
        var input = (SimplePlayerInputComponent)c;

        // ── Key Bindings ──
        DrawSection("Key Bindings");
        DrawSearchableEnumCombo("Forward##key", input.MoveForwardKey, v => input.MoveForwardKey = v);
        DrawSearchableEnumCombo("Backward##key", input.MoveBackwardKey, v => input.MoveBackwardKey = v);
        DrawSearchableEnumCombo("Left##key", input.MoveLeftKey, v => input.MoveLeftKey = v);
        DrawSearchableEnumCombo("Right##key", input.MoveRightKey, v => input.MoveRightKey = v);
        DrawSearchableEnumCombo("Jump##key", input.JumpKey, v => input.JumpKey = v);

        // ── Mouse Look ──
        DrawSection("Mouse Look");
        DrawCheckbox("Enable Mouse Look", input.EnableMouseLook, v => input.EnableMouseLook = v);

        if (input.EnableMouseLook)
        {
            DrawCheckbox("Require Mouse Button", input.RequireLookMouseButton, v => input.RequireLookMouseButton = v);
            if (input.RequireLookMouseButton)
                DrawEnumCombo("Look Mouse Button", input.LookMouseButton, v => input.LookMouseButton = v);

            DrawCheckbox("Rotate Pitch", input.RotatePitch, v => input.RotatePitch = v);
            DrawCheckbox("Use View Override", input.UseViewDirectionOverrideForCharacterLook, v => input.UseViewDirectionOverrideForCharacterLook = v);
            DrawCheckbox("Apply Pitch To Body", input.ApplyPitchToCharacterBody, v => input.ApplyPitchToCharacterBody = v);
            DrawCheckbox("Invert X", input.InvertMouseX, v => input.InvertMouseX = v);
            DrawCheckbox("Invert Y", input.InvertMouseY, v => input.InvertMouseY = v);

            float sensitivity = input.MouseSensitivity;
            DrawDragFloat("Sensitivity", ref sensitivity, 0.005f, 0f, 10f);
            input.MouseSensitivity = sensitivity;

            float minPitch = input.MinPitchDegrees;
            DrawDragFloat("Min Pitch", ref minPitch, 0.5f, -89f, 89f);
            input.MinPitchDegrees = minPitch;

            float maxPitch = input.MaxPitchDegrees;
            DrawDragFloat("Max Pitch", ref maxPitch, 0.5f, -89f, 89f);
            input.MaxPitchDegrees = maxPitch;
        }

        // ── Character Controller ──
        DrawSection("Character Controller");
        DrawCheckbox("Use Character Controller", input.UseCharacterController, v => input.UseCharacterController = v);
        DrawCheckbox("Allow Jump", input.AllowJump, v => input.AllowJump = v);

        // ── Fallback Motion ──
        if (!input.UseCharacterController)
        {
            DrawSection("Fallback Motion");
            float fallbackSpeed = input.FallbackMoveSpeed;
            DrawDragFloat("Move Speed##fallback", ref fallbackSpeed, 0.05f, 0f, 1000f);
            input.FallbackMoveSpeed = fallbackSpeed;

            float fallbackJump = input.FallbackJumpImpulse;
            DrawDragFloat("Jump Impulse##fallback", ref fallbackJump, 0.05f, 0f, 1000f);
            input.FallbackJumpImpulse = fallbackJump;
        }

        // ── Attached Camera ──
        DrawSection("Attached Camera");
        DrawCheckbox("Drive Attached Camera", input.DriveAttachedCamera, v => input.DriveAttachedCamera = v);

        if (input.DriveAttachedCamera)
        {
            var cameraProp = typeof(SimplePlayerInputComponent).GetProperty(nameof(SimplePlayerInputComponent.CameraEntity))!;
            DrawEntityReference("Camera Entity", input, cameraProp);

            var offset = input.AttachedCameraLocalOffset;
            if (DrawColoredVector3("Local Offset", ref offset, 0.05f))
                input.AttachedCameraLocalOffset = offset;

            float backDist = input.AttachedCameraBackDistance;
            DrawDragFloat("Back Distance", ref backDist, 0.05f, 0f, 100f);
            input.AttachedCameraBackDistance = backDist;
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
        float friction = collider.Friction;
        DrawDragFloat("Friction", ref friction, 0.01f, 0f, 10f);
        collider.Friction = friction;

        float restitution = collider.Restitution;
        DrawDragFloat("Restitution", ref restitution, 0.01f, 0f, 1f);
        collider.Restitution = restitution;

        var center = collider.Center;
        if (DrawColoredVector3("Center", ref center, 0.05f))
            collider.Center = center;

        int colliderCount = collider.Entity.Components.Count(component => component is ColliderComponent enabledCollider && enabledCollider.Enabled);
        if (colliderCount > 1)
            DrawWarning("Multiple enabled colliders are present. Only the first enabled collider is used.");
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
        var label = NiceLabel(prop.Name);
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
            DrawCheckbox(label, (bool)prop.GetValue(component)!, v => prop.SetValue(component, v));
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
            if (DrawColoredVector3(label, ref val, 0.1f))
                prop.SetValue(component, val);
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
            if (DrawColoredVector3(label, ref euler, 0.5f))
                prop.SetValue(component, Core.FrinkyMath.EulerToQuaternion(euler));
        }
        else if (propType == typeof(Color))
        {
            DrawColorEdit4(label, (Color)prop.GetValue(component)!, v => prop.SetValue(component, v));
        }
        else if (propType.IsEnum)
        {
            var enumValues = Enum.GetValues(propType);
            if (enumValues.Length == 0)
                return;

            var names = enumValues.Cast<object>()
                .Select(value => value.ToString() ?? value.GetType().Name)
                .ToArray();
            var currentValue = prop.GetValue(component) ?? enumValues.GetValue(0)!;
            int selectedIndex = Array.IndexOf(enumValues, currentValue);
            if (selectedIndex < 0)
                selectedIndex = 0;

            if (ImGui.Combo(label, ref selectedIndex, names, names.Length))
            {
                app.RecordUndo();
                prop.SetValue(component, enumValues.GetValue(selectedIndex));
                app.RefreshUndoBaseline();
            }
        }
        else if (propType == typeof(EntityReference))
        {
            DrawEntityReference(label, component, prop);
        }
        else
        {
            ImGui.LabelText(label, propType.Name);
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
        ImGui.Columns(2, null, false);
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
            var payload = ImGui.AcceptDragDropPayload("FRINKY_HIERARCHY_ENTITY");
            if (payload.NativePtr != null && payload.Delivery && app.DraggedEntityId.HasValue)
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

    // ─── Reusable helpers ───────────────────────────────────────────────

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
        var values = Enum.GetValues<T>();
        var names = values.Select(v => v.ToString()).ToArray();
        int selectedIndex = Array.IndexOf(values, currentValue);
        if (selectedIndex < 0) selectedIndex = 0;

        if (ImGui.Combo(label, ref selectedIndex, names, names.Length))
        {
            var app = EditorApplication.Instance;
            app.RecordUndo();
            setter(values[selectedIndex]);
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
}
