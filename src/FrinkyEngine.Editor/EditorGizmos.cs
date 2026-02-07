using System.Numerics;
using FrinkyEngine.Core.Components;
using FrinkyEngine.Core.ECS;
using Raylib_cs;

namespace FrinkyEngine.Editor;

public enum PhysicsHitboxDrawMode
{
    Off,
    SelectedOnly,
    All
}

public static class EditorGizmos
{
    private static readonly Color CameraGizmoColor = new(200, 200, 200, 255);
    private static readonly Color DirectionalLightColor = new(255, 220, 50, 255);
    private static readonly Color SelectionHighlightColor = new(255, 170, 0, 255);
    private static readonly Color PhysicsHitboxSelectedColor = new(255, 180, 50, 255);
    private static readonly Color PhysicsHitboxAllColor = new(70, 200, 255, 255);
    private static readonly Color PhysicsHitboxWarningColor = new(255, 100, 60, 255);

    public static void DrawAll(Core.Scene.Scene scene, Camera3D editorCamera)
    {
        foreach (var cam in scene.Cameras)
        {
            if (!cam.Entity.Active || !cam.Enabled) continue;
            DrawCameraGizmo(cam, editorCamera);
        }

        foreach (var light in scene.Lights)
        {
            if (!light.Entity.Active || !light.Enabled) continue;
            if (light.LightType == LightType.Directional)
                DrawDirectionalLightGizmo(light);
            else if (light.LightType == LightType.Point)
                DrawPointLightGizmo(light);
        }
    }

    public static void DrawPhysicsHitboxes(Core.Scene.Scene scene, IReadOnlyList<Entity> selectedEntities, PhysicsHitboxDrawMode mode)
    {
        if (mode == PhysicsHitboxDrawMode.Off)
            return;

        var selectedIds = new HashSet<Guid>(selectedEntities.Select(entity => entity.Id));
        IEnumerable<ColliderComponent> colliders = mode == PhysicsHitboxDrawMode.SelectedOnly
            ? selectedEntities.SelectMany(entity => entity.Components.OfType<ColliderComponent>())
            : scene.GetComponents<ColliderComponent>();

        foreach (var collider in colliders)
        {
            if (!collider.Enabled || !collider.Entity.Active)
                continue;

            var entity = collider.Entity;
            if (entity.Scene != scene)
                continue;

            var color = ResolveHitboxColor(entity, selectedIds, mode);
            DrawColliderWireframe(collider, color);
        }
    }

    private static void DrawCameraGizmo(CameraComponent cam, Camera3D editorCamera)
    {
        var pos = cam.Entity.Transform.WorldPosition;
        var forward = cam.Entity.Transform.Forward;
        var right = cam.Entity.Transform.Right;
        var up = cam.Entity.Transform.Up;

        float distance = Vector3.Distance(editorCamera.Position, pos);
        float scale = Math.Clamp(distance * 0.05f, 0.3f, 2f);

        float nearDist = 0.5f * scale;
        float farDist = 2.0f * scale;
        float fovRad = cam.FieldOfView * MathF.PI / 180f;
        float aspect = 16f / 9f;

        float nearH = MathF.Tan(fovRad * 0.5f) * nearDist;
        float nearW = nearH * aspect;
        float farH = MathF.Tan(fovRad * 0.5f) * farDist;
        float farW = farH * aspect;

        // Near plane corners
        var nc = pos + forward * nearDist;
        var ntl = nc + up * nearH - right * nearW;
        var ntr = nc + up * nearH + right * nearW;
        var nbl = nc - up * nearH - right * nearW;
        var nbr = nc - up * nearH + right * nearW;

        // Far plane corners
        var fc = pos + forward * farDist;
        var ftl = fc + up * farH - right * farW;
        var ftr = fc + up * farH + right * farW;
        var fbl = fc - up * farH - right * farW;
        var fbr = fc - up * farH + right * farW;

        // Near rectangle
        Raylib.DrawLine3D(ntl, ntr, CameraGizmoColor);
        Raylib.DrawLine3D(ntr, nbr, CameraGizmoColor);
        Raylib.DrawLine3D(nbr, nbl, CameraGizmoColor);
        Raylib.DrawLine3D(nbl, ntl, CameraGizmoColor);

        // Far rectangle
        Raylib.DrawLine3D(ftl, ftr, CameraGizmoColor);
        Raylib.DrawLine3D(ftr, fbr, CameraGizmoColor);
        Raylib.DrawLine3D(fbr, fbl, CameraGizmoColor);
        Raylib.DrawLine3D(fbl, ftl, CameraGizmoColor);

        // Connecting edges
        Raylib.DrawLine3D(ntl, ftl, CameraGizmoColor);
        Raylib.DrawLine3D(ntr, ftr, CameraGizmoColor);
        Raylib.DrawLine3D(nbl, fbl, CameraGizmoColor);
        Raylib.DrawLine3D(nbr, fbr, CameraGizmoColor);

        // Origin to near plane corners
        Raylib.DrawLine3D(pos, ntl, CameraGizmoColor);
        Raylib.DrawLine3D(pos, ntr, CameraGizmoColor);
        Raylib.DrawLine3D(pos, nbl, CameraGizmoColor);
        Raylib.DrawLine3D(pos, nbr, CameraGizmoColor);
    }

    private static void DrawDirectionalLightGizmo(LightComponent light)
    {
        var pos = light.Entity.Transform.WorldPosition;
        var forward = light.Entity.Transform.Forward;

        float arrowLength = 2.0f;
        var end = pos + forward * arrowLength;

        // Main arrow line
        Raylib.DrawLine3D(pos, end, DirectionalLightColor);

        // Arrowhead lines
        var right = light.Entity.Transform.Right;
        var up = light.Entity.Transform.Up;
        float headSize = 0.3f;
        var headBase = pos + forward * (arrowLength - headSize);

        Raylib.DrawLine3D(end, headBase + right * headSize * 0.5f, DirectionalLightColor);
        Raylib.DrawLine3D(end, headBase - right * headSize * 0.5f, DirectionalLightColor);
        Raylib.DrawLine3D(end, headBase + up * headSize * 0.5f, DirectionalLightColor);
        Raylib.DrawLine3D(end, headBase - up * headSize * 0.5f, DirectionalLightColor);

        // Small rays emanating from origin
        float rayLen = 0.5f;
        float offset = 0.4f;
        var rayPositions = new[]
        {
            pos + right * offset,
            pos - right * offset,
            pos + up * offset,
            pos - up * offset
        };
        foreach (var rp in rayPositions)
        {
            Raylib.DrawLine3D(rp, rp + forward * rayLen, DirectionalLightColor);
        }
    }

    private static void DrawPointLightGizmo(LightComponent light)
    {
        var pos = light.Entity.Transform.WorldPosition;
        var color = new Color(light.LightColor.R, light.LightColor.G, light.LightColor.B, (byte)128);
        Raylib.DrawSphereWires(pos, light.Range, 8, 8, color);
    }

    public static void DrawSelectionFallbackHighlight(Entity? selected)
    {
        if (selected == null || !selected.Active) return;

        var renderable = selected.GetComponent<RenderableComponent>();
        if (renderable != null && renderable.Enabled) return;

        // Non-renderable entities (cameras, lights): draw a small wireframe cube
        var camera = selected.GetComponent<CameraComponent>();
        var light = selected.GetComponent<LightComponent>();
        bool hasVisual = (camera != null && camera.Enabled)
                      || (light != null && light.Enabled);
        if (hasVisual)
        {
            var pos = selected.Transform.WorldPosition;
            var halfExt = new Vector3(0.5f);
            Raylib.DrawBoundingBox(
                new BoundingBox(pos - halfExt, pos + halfExt),
                SelectionHighlightColor);
        }
    }

    private static Color ResolveHitboxColor(Entity entity, HashSet<Guid> selectedIds, PhysicsHitboxDrawMode mode)
    {
        var rigidbody = entity.GetComponent<RigidbodyComponent>();
        bool parentedRigidbody = rigidbody is { Enabled: true } && entity.Transform.Parent != null;
        if (parentedRigidbody)
            return PhysicsHitboxWarningColor;

        if (mode == PhysicsHitboxDrawMode.SelectedOnly || selectedIds.Contains(entity.Id))
            return PhysicsHitboxSelectedColor;

        return PhysicsHitboxAllColor;
    }

    private static void DrawColliderWireframe(ColliderComponent collider, Color color)
    {
        switch (collider)
        {
            case BoxColliderComponent box:
                DrawBoxCollider(box, color);
                break;
            case SphereColliderComponent sphere:
                DrawSphereCollider(sphere, color);
                break;
            case CapsuleColliderComponent capsule:
                DrawCapsuleCollider(capsule, color);
                break;
        }
    }

    private static void DrawBoxCollider(BoxColliderComponent collider, Color color)
    {
        var transform = collider.Entity.Transform;
        if (!TryGetWorldBasis(transform, out var position, out var rotation, out var scale))
            return;

        var center = ComputeWorldCenter(collider, position, rotation, scale);
        var halfExtents = new Vector3(
            MathF.Max(0.0005f, collider.Size.X * scale.X * 0.5f),
            MathF.Max(0.0005f, collider.Size.Y * scale.Y * 0.5f),
            MathF.Max(0.0005f, collider.Size.Z * scale.Z * 0.5f));

        var axisX = Vector3.Normalize(Vector3.Transform(Vector3.UnitX, rotation));
        var axisY = Vector3.Normalize(Vector3.Transform(Vector3.UnitY, rotation));
        var axisZ = Vector3.Normalize(Vector3.Transform(Vector3.UnitZ, rotation));

        var corners = new Vector3[8];
        int index = 0;
        for (int x = -1; x <= 1; x += 2)
        {
            for (int y = -1; y <= 1; y += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    corners[index++] = center
                        + axisX * (halfExtents.X * x)
                        + axisY * (halfExtents.Y * y)
                        + axisZ * (halfExtents.Z * z);
                }
            }
        }

        DrawEdge(corners, 0, 1, color);
        DrawEdge(corners, 0, 2, color);
        DrawEdge(corners, 0, 4, color);
        DrawEdge(corners, 1, 3, color);
        DrawEdge(corners, 1, 5, color);
        DrawEdge(corners, 2, 3, color);
        DrawEdge(corners, 2, 6, color);
        DrawEdge(corners, 3, 7, color);
        DrawEdge(corners, 4, 5, color);
        DrawEdge(corners, 4, 6, color);
        DrawEdge(corners, 5, 7, color);
        DrawEdge(corners, 6, 7, color);
    }

    private static void DrawSphereCollider(SphereColliderComponent collider, Color color)
    {
        var transform = collider.Entity.Transform;
        if (!TryGetWorldBasis(transform, out var position, out var rotation, out var scale))
            return;

        var center = ComputeWorldCenter(collider, position, rotation, scale);
        float radiusScale = MathF.Max(scale.X, MathF.Max(scale.Y, scale.Z));
        float radius = MathF.Max(0.0005f, collider.Radius * radiusScale);
        Raylib.DrawSphereWires(center, radius, 10, 10, color);
    }

    private static void DrawCapsuleCollider(CapsuleColliderComponent collider, Color color)
    {
        var transform = collider.Entity.Transform;
        if (!TryGetWorldBasis(transform, out var position, out var rotation, out var scale))
            return;

        var center = ComputeWorldCenter(collider, position, rotation, scale);

        float radiusScale = MathF.Max(scale.X, scale.Z);
        float radius = MathF.Max(0.0005f, collider.Radius * radiusScale);
        float halfLength = MathF.Max(0.0005f, collider.Length * scale.Y * 0.5f);

        var up = Vector3.Normalize(Vector3.Transform(Vector3.UnitY, rotation));
        var right = Vector3.Normalize(Vector3.Transform(Vector3.UnitX, rotation));
        var forward = Vector3.Normalize(Vector3.Transform(Vector3.UnitZ, rotation));

        var top = center + up * halfLength;
        var bottom = center - up * halfLength;

        Raylib.DrawSphereWires(top, radius, 10, 10, color);
        Raylib.DrawSphereWires(bottom, radius, 10, 10, color);

        Raylib.DrawLine3D(top + right * radius, bottom + right * radius, color);
        Raylib.DrawLine3D(top - right * radius, bottom - right * radius, color);
        Raylib.DrawLine3D(top + forward * radius, bottom + forward * radius, color);
        Raylib.DrawLine3D(top - forward * radius, bottom - forward * radius, color);
    }

    private static bool TryGetWorldBasis(TransformComponent transform, out Vector3 position, out Quaternion rotation, out Vector3 absScale)
    {
        if (Matrix4x4.Decompose(transform.WorldMatrix, out var scale, out rotation, out position))
        {
            absScale = new Vector3(MathF.Abs(scale.X), MathF.Abs(scale.Y), MathF.Abs(scale.Z));
            absScale.X = MathF.Max(absScale.X, 0.0001f);
            absScale.Y = MathF.Max(absScale.Y, 0.0001f);
            absScale.Z = MathF.Max(absScale.Z, 0.0001f);
            rotation = Quaternion.Normalize(rotation);
            return true;
        }

        position = transform.WorldPosition;
        rotation = Quaternion.Normalize(transform.WorldRotation);
        absScale = Vector3.One;
        return true;
    }

    private static Vector3 ComputeWorldCenter(ColliderComponent collider, Vector3 worldPosition, Quaternion worldRotation, Vector3 worldScale)
    {
        var scaledCenter = new Vector3(
            collider.Center.X * worldScale.X,
            collider.Center.Y * worldScale.Y,
            collider.Center.Z * worldScale.Z);
        return worldPosition + Vector3.Transform(scaledCenter, worldRotation);
    }

    private static void DrawEdge(Vector3[] corners, int indexA, int indexB, Color color)
    {
        Raylib.DrawLine3D(corners[indexA], corners[indexB], color);
    }
}
