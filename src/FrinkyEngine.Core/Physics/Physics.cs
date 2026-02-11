using System.Numerics;

namespace FrinkyEngine.Core.Physics;

/// <summary>
/// Static entry point for physics queries such as raycasting.
/// </summary>
public static class Physics
{
    internal static Scene.Scene? CurrentScene { get; set; }

    /// <summary>
    /// Casts a ray and returns the closest hit, if any.
    /// </summary>
    /// <param name="origin">World-space origin of the ray.</param>
    /// <param name="direction">Direction of the ray (does not need to be normalized).</param>
    /// <param name="maxDistance">Maximum distance the ray can travel.</param>
    /// <param name="hit">Information about the closest hit, if the method returns <c>true</c>.</param>
    /// <param name="includeTriggers">When <c>true</c>, trigger colliders are included in the test.</param>
    /// <returns><c>true</c> if the ray hit something within <paramref name="maxDistance"/>.</returns>
    public static bool Raycast(Vector3 origin, Vector3 direction, float maxDistance, out RaycastHit hit, bool includeTriggers = false)
    {
        hit = default;
        var physics = CurrentScene?.PhysicsSystem;
        if (physics == null)
            return false;

        return physics.Raycast(origin, direction, maxDistance, out hit, includeTriggers);
    }

    /// <summary>
    /// Casts a ray and returns all hits along it.
    /// </summary>
    /// <param name="origin">World-space origin of the ray.</param>
    /// <param name="direction">Direction of the ray (does not need to be normalized).</param>
    /// <param name="maxDistance">Maximum distance the ray can travel.</param>
    /// <param name="includeTriggers">When <c>true</c>, trigger colliders are included in the test.</param>
    /// <returns>A list of all hits along the ray, unordered.</returns>
    public static List<RaycastHit> RaycastAll(Vector3 origin, Vector3 direction, float maxDistance, bool includeTriggers = false)
    {
        var physics = CurrentScene?.PhysicsSystem;
        if (physics == null)
            return new List<RaycastHit>();

        return physics.RaycastAll(origin, direction, maxDistance, includeTriggers);
    }
}
