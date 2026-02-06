using System.Numerics;
using FrinkyEngine.Core.Components;
using FrinkyEngine.Core.ECS;
using Raylib_cs;

namespace FrinkyEngine.Editor;

public class PickingSystem
{
    private const float IconHalfExtent = 0.5f;

    public Entity? Pick(Core.Scene.Scene scene, Camera3D camera, Vector2 mousePos, Vector2 viewportSize)
    {
        var ray = RaycastUtils.GetViewportRay(camera, mousePos, viewportSize);

        Entity? closest = null;
        float closestDist = float.MaxValue;

        foreach (var entity in scene.Entities)
        {
            if (!entity.Active) continue;

            var renderable = entity.GetComponent<RenderableComponent>();
            if (renderable != null && renderable.Enabled)
            {
                var bb = renderable.GetWorldBoundingBox();
                if (bb.HasValue)
                {
                    var collision = Raylib.GetRayCollisionBox(ray, bb.Value);
                    if (collision.Hit && collision.Distance < closestDist)
                    {
                        closestDist = collision.Distance;
                        closest = entity;
                    }
                }
                continue;
            }

            // Non-renderable entities with visual editor presence (cameras, lights)
            bool hasVisualComponent = entity.GetComponent<CameraComponent>() != null
                                   || entity.GetComponent<LightComponent>() != null;
            if (hasVisualComponent)
            {
                var pos = entity.Transform.WorldPosition;
                var syntheticBB = new BoundingBox(
                    pos - new Vector3(IconHalfExtent),
                    pos + new Vector3(IconHalfExtent));

                var collision = Raylib.GetRayCollisionBox(ray, syntheticBB);
                if (collision.Hit && collision.Distance < closestDist)
                {
                    closestDist = collision.Distance;
                    closest = entity;
                }
            }
        }

        return closest;
    }
}
