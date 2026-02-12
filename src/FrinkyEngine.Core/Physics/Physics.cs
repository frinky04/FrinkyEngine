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
    /// <param name="raycastParams">Optional filtering options for the raycast.</param>
    /// <returns><c>true</c> if the ray hit something within <paramref name="maxDistance"/>.</returns>
    public static bool Raycast(Vector3 origin, Vector3 direction, float maxDistance, out RaycastHit hit, RaycastParams? raycastParams = null)
    {
        hit = default;
        var physics = CurrentScene?.PhysicsSystem;
        if (physics == null)
            return false;

        return physics.Raycast(origin, direction, maxDistance, out hit, raycastParams ?? default);
    }

    /// <summary>
    /// Casts a ray between two points and returns the closest hit, if any.
    /// </summary>
    /// <param name="from">World-space start point.</param>
    /// <param name="to">World-space end point.</param>
    /// <param name="hit">Information about the closest hit, if the method returns <c>true</c>.</param>
    /// <param name="raycastParams">Optional filtering options for the raycast.</param>
    /// <returns><c>true</c> if the ray hit something between <paramref name="from"/> and <paramref name="to"/>.</returns>
    public static bool Raycast(Vector3 from, Vector3 to, out RaycastHit hit, RaycastParams? raycastParams = null)
    {
        var direction = to - from;
        var maxDistance = direction.Length();
        return Raycast(from, direction, maxDistance, out hit, raycastParams);
    }

    /// <summary>
    /// Casts a ray and returns all hits along it.
    /// </summary>
    /// <param name="origin">World-space origin of the ray.</param>
    /// <param name="direction">Direction of the ray (does not need to be normalized).</param>
    /// <param name="maxDistance">Maximum distance the ray can travel.</param>
    /// <param name="raycastParams">Optional filtering options for the raycast.</param>
    /// <returns>A list of all hits along the ray, unordered.</returns>
    public static List<RaycastHit> RaycastAll(Vector3 origin, Vector3 direction, float maxDistance, RaycastParams? raycastParams = null)
    {
        var physics = CurrentScene?.PhysicsSystem;
        if (physics == null)
            return new List<RaycastHit>();

        return physics.RaycastAll(origin, direction, maxDistance, raycastParams ?? default);
    }

    /// <summary>
    /// Casts a ray between two points and returns all hits along it.
    /// </summary>
    /// <param name="from">World-space start point.</param>
    /// <param name="to">World-space end point.</param>
    /// <param name="raycastParams">Optional filtering options for the raycast.</param>
    /// <returns>A list of all hits between <paramref name="from"/> and <paramref name="to"/>, unordered.</returns>
    public static List<RaycastHit> RaycastAll(Vector3 from, Vector3 to, RaycastParams? raycastParams = null)
    {
        var direction = to - from;
        var maxDistance = direction.Length();
        return RaycastAll(from, direction, maxDistance, raycastParams);
    }
}
