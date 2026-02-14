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
        if (!EditorGizmos.TryGetWorldBasis(box.Entity.Transform, out var worldPosition, out var worldRotation, out var absScale))
            return;

        var center = EditorGizmos.ComputeWorldCenter(box, worldPosition, worldRotation, absScale);
        var colliderWorldScale = new Vector3(
            MathF.Max(0.001f, box.Size.X * absScale.X),
            MathF.Max(0.001f, box.Size.Y * absScale.Y),
            MathF.Max(0.001f, box.Size.Z * absScale.Z));

        // In ImGuizmo, localBounds is input-only; Bounds manipulates the matrix transform.
        // Use a unit AABB and read updated size from matrix scale after manipulation.
        var objectMatrix = Matrix4x4.CreateScale(colliderWorldScale)
            * Matrix4x4.CreateFromQuaternion(worldRotation)
            * Matrix4x4.CreateTranslation(center);

        var originalMatrix = objectMatrix;
        var deltaMatrix = Matrix4x4.Identity;
        float[] localBounds = { -0.5f, -0.5f, -0.5f, 0.5f, 0.5f, 0.5f };

        bool changed;
        fixed (float* boundsPtr = localBounds)
        {
            changed = ImGuizmo.Manipulate(
                (float*)&view, (float*)&proj,
                ImGuizmoOperation.Bounds | ImGuizmoOperation.Translate,
                ImGuizmoMode.Local,
                (float*)&objectMatrix, (float*)&deltaMatrix,
                (float*)null, boundsPtr, (float*)null);
        }

        bool shouldApply = changed || ImGuizmo.IsUsing();
        if (shouldApply)
        {
            // Bounds mode can report interaction while leaving matrix unchanged on some frames.
            // When that happens, consume deltaMatrix instead.
            var resolvedMatrix = objectMatrix;
            if (MatrixNearlyEqual(resolvedMatrix, originalMatrix) && !MatrixNearlyEqual(deltaMatrix, Matrix4x4.Identity))
                resolvedMatrix = deltaMatrix * originalMatrix;

            if (Matrix4x4.Decompose(resolvedMatrix, out var newScale, out _, out var newPos))
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

    private static bool MatrixNearlyEqual(in Matrix4x4 a, in Matrix4x4 b, float epsilon = 1e-5f)
    {
        return MathF.Abs(a.M11 - b.M11) < epsilon && MathF.Abs(a.M12 - b.M12) < epsilon && MathF.Abs(a.M13 - b.M13) < epsilon && MathF.Abs(a.M14 - b.M14) < epsilon
            && MathF.Abs(a.M21 - b.M21) < epsilon && MathF.Abs(a.M22 - b.M22) < epsilon && MathF.Abs(a.M23 - b.M23) < epsilon && MathF.Abs(a.M24 - b.M24) < epsilon
            && MathF.Abs(a.M31 - b.M31) < epsilon && MathF.Abs(a.M32 - b.M32) < epsilon && MathF.Abs(a.M33 - b.M33) < epsilon && MathF.Abs(a.M34 - b.M34) < epsilon
            && MathF.Abs(a.M41 - b.M41) < epsilon && MathF.Abs(a.M42 - b.M42) < epsilon && MathF.Abs(a.M43 - b.M43) < epsilon && MathF.Abs(a.M44 - b.M44) < epsilon;
    }

    private static unsafe void ManipulateSphereCollider(SphereColliderComponent sphere, Matrix4x4 view, Matrix4x4 proj)
    {
        if (!EditorGizmos.TryGetWorldBasis(sphere.Entity.Transform, out var worldPosition, out var worldRotation, out var absScale))
            return;

        float radiusScale = MathF.Max(absScale.X, MathF.Max(absScale.Y, absScale.Z));
        var center = EditorGizmos.ComputeWorldCenter(sphere, worldPosition, worldRotation, absScale);
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
        if (!EditorGizmos.TryGetWorldBasis(capsule.Entity.Transform, out var worldPosition, out var worldRotation, out var absScale))
            return;

        float radiusScale = MathF.Max(absScale.X, absScale.Z);
        var center = EditorGizmos.ComputeWorldCenter(capsule, worldPosition, worldRotation, absScale);

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

}
