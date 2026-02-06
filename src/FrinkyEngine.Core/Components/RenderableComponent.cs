using System.Numerics;
using FrinkyEngine.Core.ECS;
using Raylib_cs;

namespace FrinkyEngine.Core.Components;

public abstract class RenderableComponent : Component
{
    public Color Tint { get; set; } = new(255, 255, 255, 255);

    internal Model? RenderModel { get; set; }

    internal abstract void EnsureModelReady();

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
