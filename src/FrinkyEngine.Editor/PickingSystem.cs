using System.Numerics;
using FrinkyEngine.Core.Components;
using FrinkyEngine.Core.ECS;
using Raylib_cs;

namespace FrinkyEngine.Editor;

public class PickingSystem
{
    private const float IconPickRadius = 0.5f;
    private const float HitDistanceEpsilon = 1e-5f;

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
                    var broadphaseCollision = Raylib.GetRayCollisionBox(ray, bb.Value);
                    if (broadphaseCollision.Hit)
                    {
                        var preciseCollision = renderable.GetWorldRayCollision(ray, out bool hasMeshData, frontFacesOnly: true);
                        if (preciseCollision.HasValue)
                        {
                            float hitDistance = preciseCollision.Value.Distance;
                            if (hitDistance > HitDistanceEpsilon && hitDistance < closestDist)
                            {
                                closestDist = hitDistance;
                                closest = entity;
                            }
                        }
                        else if (!hasMeshData)
                        {
                            // Fallback path if a renderable cannot provide mesh geometry for narrowphase testing.
                            float hitDistance = broadphaseCollision.Distance;
                            if (hitDistance > HitDistanceEpsilon && hitDistance < closestDist)
                            {
                                closestDist = hitDistance;
                                closest = entity;
                            }
                        }
                    }
                }
                continue;
            }

            // Non-renderable entities with visual editor presence (cameras, lights)
            var cameraComponent = entity.GetComponent<CameraComponent>();
            var lightComponent = entity.GetComponent<LightComponent>();
            bool hasVisualComponent = (cameraComponent != null && cameraComponent.Enabled)
                                   || (lightComponent != null && lightComponent.Enabled);
            if (hasVisualComponent)
            {
                var pos = entity.Transform.WorldPosition;
                var collision = Raylib.GetRayCollisionSphere(ray, pos, IconPickRadius);
                if (collision.Hit && collision.Distance > HitDistanceEpsilon && collision.Distance < closestDist)
                {
                    closestDist = collision.Distance;
                    closest = entity;
                }
            }
        }

        return closest;
    }
}
