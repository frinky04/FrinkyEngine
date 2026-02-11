using System.Numerics;
using FrinkyEngine.Core.ECS;
using Raylib_cs;

namespace FrinkyEngine.Core.Components;

/// <summary>
/// Abstract base class for components that can be drawn by the <see cref="Rendering.SceneRenderer"/>.
/// Provides ray-collision testing and world-space bounding box computation.
/// </summary>
public abstract class RenderableComponent : Component
{
    private const float HitDistanceEpsilon = 1e-5f;
    private const float FrontFaceDotThreshold = -1e-4f;

    internal Model? RenderModel { get; set; }

    /// <summary>
    /// Marks the internal render model as stale so it will be rebuilt before the next draw.
    /// </summary>
    public virtual void Invalidate() { RenderModel = null; }

    internal abstract void EnsureModelReady();

    /// <summary>
    /// Casts a ray against this renderable's mesh in world space.
    /// </summary>
    /// <param name="ray">The ray to test.</param>
    /// <param name="frontFacesOnly">When <c>true</c>, ignores back-facing triangles.</param>
    /// <returns>The closest hit, or <c>null</c> if the ray misses.</returns>
    public RayCollision? GetWorldRayCollision(Ray ray, bool frontFacesOnly = true)
    {
        return GetWorldRayCollision(ray, out _, frontFacesOnly);
    }

    /// <summary>
    /// Casts a ray against this renderable's mesh in world space, also reporting whether mesh data was available.
    /// </summary>
    /// <param name="ray">The ray to test.</param>
    /// <param name="hasMeshData">Set to <c>true</c> if the model has mesh data to test against.</param>
    /// <param name="frontFacesOnly">When <c>true</c>, ignores back-facing triangles.</param>
    /// <returns>The closest hit, or <c>null</c> if the ray misses.</returns>
    public RayCollision? GetWorldRayCollision(Ray ray, out bool hasMeshData, bool frontFacesOnly = true)
    {
        EnsureModelReady();
        if (!RenderModel.HasValue)
        {
            hasMeshData = false;
            return null;
        }

        var model = RenderModel.Value;
        if (model.MeshCount <= 0)
        {
            hasMeshData = false;
            return null;
        }

        hasMeshData = true;
        var worldTransform = Matrix4x4.Transpose(Entity.Transform.WorldMatrix);

        RayCollision? closestCollision = null;
        float closestDistance = float.MaxValue;

        unsafe
        {
            for (int m = 0; m < model.MeshCount; m++)
            {
                var collision = Raylib.GetRayCollisionMesh(ray, model.Meshes[m], worldTransform);
                if (!collision.Hit) continue;
                if (collision.Distance <= HitDistanceEpsilon) continue;

                if (frontFacesOnly)
                {
                    if (collision.Normal.LengthSquared() > 1e-8f)
                    {
                        float facing = Vector3.Dot(collision.Normal, ray.Direction);
                        if (facing >= FrontFaceDotThreshold)
                            continue;
                    }
                }

                if (collision.Distance < closestDistance)
                {
                    closestDistance = collision.Distance;
                    closestCollision = collision;
                }
            }
        }

        return closestCollision;
    }

    /// <summary>
    /// Computes the axis-aligned bounding box of this renderable in world space.
    /// </summary>
    /// <returns>The world-space bounding box, or <c>null</c> if no mesh data is available.</returns>
    public BoundingBox? GetWorldBoundingBox()
    {
        EnsureModelReady();
        if (!RenderModel.HasValue) return null;

        var model = RenderModel.Value;
        if (model.MeshCount <= 0) return null;

        var worldMatrix = Entity.Transform.WorldMatrix;
        float minX = float.MaxValue, minY = float.MaxValue, minZ = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue, maxZ = float.MinValue;

        unsafe
        {
            for (int m = 0; m < model.MeshCount; m++)
            {
                var meshBB = Raylib.GetMeshBoundingBox(model.Meshes[m]);
                // Transform all 8 corners of the local AABB
                var bMin = meshBB.Min;
                var bMax = meshBB.Max;
                for (int c = 0; c < 8; c++)
                {
                    var corner = new Vector3(
                        (c & 1) == 0 ? bMin.X : bMax.X,
                        (c & 2) == 0 ? bMin.Y : bMax.Y,
                        (c & 4) == 0 ? bMin.Z : bMax.Z);

                    var world = Vector3.Transform(corner, worldMatrix);
                    if (world.X < minX) minX = world.X;
                    if (world.Y < minY) minY = world.Y;
                    if (world.Z < minZ) minZ = world.Z;
                    if (world.X > maxX) maxX = world.X;
                    if (world.Y > maxY) maxY = world.Y;
                    if (world.Z > maxZ) maxZ = world.Z;
                }
            }
        }

        return new BoundingBox(
            new Vector3(minX, minY, minZ),
            new Vector3(maxX, maxY, maxZ));
    }
}
