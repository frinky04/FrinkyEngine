using System.Numerics;
using FrinkyEngine.Core.Components;
using FrinkyEngine.Core.ECS;
using Hexa.NET.ImGuizmo;
using Raylib_cs;

namespace FrinkyEngine.Editor;

public class ColliderEditSystem
{
    public bool IsDragging { get; private set; }

    public unsafe void DrawAndUpdate(
        Camera3D camera,
        Entity? selectedEntity,
        Vector2 viewportScreenPos,
        Vector2 viewportSize)
    {
        IsDragging = false;

        if (selectedEntity == null)
            return;

        var collider = selectedEntity.GetComponent<ColliderComponent>();
        if (collider == null || !collider.Enabled)
            return;

        ImGuizmo.SetRect(viewportScreenPos.X, viewportScreenPos.Y, viewportSize.X, viewportSize.Y);
        ImGuizmo.SetOrthographic(false);
        ImGuizmo.SetDrawlist();
        ImGuizmo.SetID(2000);

        var view = Matrix4x4.CreateLookAt(camera.Position, camera.Target, camera.Up);
        float aspect = viewportSize.X / viewportSize.Y;
        var proj = Matrix4x4.CreatePerspectiveFieldOfView(
            camera.FovY * FrinkyEngine.Core.FrinkyMath.Deg2Rad, aspect, 0.01f, 1000f);

        switch (collider)
        {
            case BoxColliderComponent box:
                ManipulateBoxCollider(box, view, proj);
                break;
            case SphereColliderComponent sphere:
                ManipulateSphereCollider(sphere, view, proj);
                break;
            case CapsuleColliderComponent capsule:
                ManipulateCapsuleCollider(capsule, view, proj);
                break;
        }

        IsDragging = ImGuizmo.IsUsing();

        ImGuizmo.SetID(0);
    }

    private static unsafe void ManipulateBoxCollider(BoxColliderComponent box, Matrix4x4 view, Matrix4x4 proj)
    {
        var transform = box.Entity.Transform;
        if (!Matrix4x4.Decompose(transform.WorldMatrix, out var worldScale, out var worldRotation, out var worldPosition))
            return;

        worldRotation = Quaternion.Normalize(worldRotation);
        var absScale = new Vector3(MathF.Abs(worldScale.X), MathF.Abs(worldScale.Y), MathF.Abs(worldScale.Z));
        absScale = Vector3.Max(absScale, new Vector3(0.0001f));

        var center = ComputeWorldCenter(box, worldPosition, worldRotation, absScale);
        var colliderWorldScale = new Vector3(
            box.Size.X * absScale.X,
            box.Size.Y * absScale.Y,
            box.Size.Z * absScale.Z);

        var objectMatrix = Matrix4x4.CreateScale(colliderWorldScale)
            * Matrix4x4.CreateFromQuaternion(worldRotation)
            * Matrix4x4.CreateTranslation(center);

        var deltaMatrix = Matrix4x4.Identity;

        bool changed = ImGuizmo.Manipulate(
            (float*)&view, (float*)&proj,
            ImGuizmoOperation.Scale | ImGuizmoOperation.Translate,
            ImGuizmoMode.Local,
            (float*)&objectMatrix, (float*)&deltaMatrix,
            (float*)null, (float*)null, (float*)null);

        if (changed)
        {
            if (Matrix4x4.Decompose(objectMatrix, out var newScale, out _, out var newPos))
            {
                var invRotation = Quaternion.Inverse(worldRotation);
                var localOffset = Vector3.Transform(newPos - worldPosition, invRotation);
                box.Center = new Vector3(
                    localOffset.X / absScale.X,
                    localOffset.Y / absScale.Y,
                    localOffset.Z / absScale.Z);

                box.Size = new Vector3(
                    MathF.Max(0.001f, MathF.Abs(newScale.X) / absScale.X),
                    MathF.Max(0.001f, MathF.Abs(newScale.Y) / absScale.Y),
                    MathF.Max(0.001f, MathF.Abs(newScale.Z) / absScale.Z));
            }
        }
    }

    private static unsafe void ManipulateSphereCollider(SphereColliderComponent sphere, Matrix4x4 view, Matrix4x4 proj)
    {
        var transform = sphere.Entity.Transform;
        if (!Matrix4x4.Decompose(transform.WorldMatrix, out var worldScale, out var worldRotation, out var worldPosition))
            return;

        worldRotation = Quaternion.Normalize(worldRotation);
        var absScale = new Vector3(MathF.Abs(worldScale.X), MathF.Abs(worldScale.Y), MathF.Abs(worldScale.Z));
        absScale = Vector3.Max(absScale, new Vector3(0.0001f));

        float radiusScale = MathF.Max(absScale.X, MathF.Max(absScale.Y, absScale.Z));
        var center = ComputeWorldCenter(sphere, worldPosition, worldRotation, absScale);
        float worldRadius = sphere.Radius * radiusScale;

        // Use uniform scale for sphere
        var objectMatrix = Matrix4x4.CreateScale(worldRadius)
            * Matrix4x4.CreateFromQuaternion(worldRotation)
            * Matrix4x4.CreateTranslation(center);

        var deltaMatrix = Matrix4x4.Identity;

        bool changed = ImGuizmo.Manipulate(
            (float*)&view, (float*)&proj,
            ImGuizmoOperation.Scale | ImGuizmoOperation.Translate,
            ImGuizmoMode.Local,
            (float*)&objectMatrix, (float*)&deltaMatrix,
            (float*)null, (float*)null, (float*)null);

        if (changed)
        {
            if (Matrix4x4.Decompose(objectMatrix, out var newScale, out _, out var newPos))
            {
                var invRotation = Quaternion.Inverse(worldRotation);
                var localOffset = Vector3.Transform(newPos - worldPosition, invRotation);
                sphere.Center = new Vector3(
                    localOffset.X / absScale.X,
                    localOffset.Y / absScale.Y,
                    localOffset.Z / absScale.Z);

                // Use max of XYZ for uniform sphere radius
                float maxScale = MathF.Max(MathF.Abs(newScale.X), MathF.Max(MathF.Abs(newScale.Y), MathF.Abs(newScale.Z)));
                sphere.Radius = MathF.Max(0.001f, maxScale / radiusScale);
            }
        }
    }

    private static unsafe void ManipulateCapsuleCollider(CapsuleColliderComponent capsule, Matrix4x4 view, Matrix4x4 proj)
    {
        var transform = capsule.Entity.Transform;
        if (!Matrix4x4.Decompose(transform.WorldMatrix, out var worldScale, out var worldRotation, out var worldPosition))
            return;

        worldRotation = Quaternion.Normalize(worldRotation);
        var absScale = new Vector3(MathF.Abs(worldScale.X), MathF.Abs(worldScale.Y), MathF.Abs(worldScale.Z));
        absScale = Vector3.Max(absScale, new Vector3(0.0001f));

        float radiusScale = MathF.Max(absScale.X, absScale.Z);
        var center = ComputeWorldCenter(capsule, worldPosition, worldRotation, absScale);

        // Encode capsule as scale: X/Z = radius, Y = length
        var capsuleScale = new Vector3(
            capsule.Radius * radiusScale,
            capsule.Length * absScale.Y,
            capsule.Radius * radiusScale);

        var objectMatrix = Matrix4x4.CreateScale(capsuleScale)
            * Matrix4x4.CreateFromQuaternion(worldRotation)
            * Matrix4x4.CreateTranslation(center);

        var deltaMatrix = Matrix4x4.Identity;

        bool changed = ImGuizmo.Manipulate(
            (float*)&view, (float*)&proj,
            ImGuizmoOperation.Scale | ImGuizmoOperation.Translate,
            ImGuizmoMode.Local,
            (float*)&objectMatrix, (float*)&deltaMatrix,
            (float*)null, (float*)null, (float*)null);

        if (changed)
        {
            if (Matrix4x4.Decompose(objectMatrix, out var newScale, out _, out var newPos))
            {
                var invRotation = Quaternion.Inverse(worldRotation);
                var localOffset = Vector3.Transform(newPos - worldPosition, invRotation);
                capsule.Center = new Vector3(
                    localOffset.X / absScale.X,
                    localOffset.Y / absScale.Y,
                    localOffset.Z / absScale.Z);

                float newRadius = (MathF.Abs(newScale.X) + MathF.Abs(newScale.Z)) * 0.5f / radiusScale;
                capsule.Radius = MathF.Max(0.001f, newRadius);
                capsule.Length = MathF.Max(0.001f, MathF.Abs(newScale.Y) / absScale.Y);
            }
        }
    }

    private static Vector3 ComputeWorldCenter(ColliderComponent collider, Vector3 worldPosition, Quaternion worldRotation, Vector3 worldScale)
    {
        var scaledCenter = new Vector3(
            collider.Center.X * worldScale.X,
            collider.Center.Y * worldScale.Y,
            collider.Center.Z * worldScale.Z);
        return worldPosition + Vector3.Transform(scaledCenter, worldRotation);
    }
}
