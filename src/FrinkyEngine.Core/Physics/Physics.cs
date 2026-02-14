using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using FrinkyEngine.Core.ECS;

namespace FrinkyEngine.Core.Physics;

/// <summary>
/// Static entry point for physics queries such as raycasting, shape casts, and overlap tests.
/// </summary>
public static class Physics
{
    internal static Scene.Scene? CurrentScene { get; set; }

    // --- Raycast ---

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

    // --- Sphere Cast ---

    /// <summary>
    /// Sweeps a sphere along a direction and returns the closest hit, if any.
    /// </summary>
    /// <param name="origin">World-space center of the sphere at the start of the sweep.</param>
    /// <param name="radius">Radius of the sphere.</param>
    /// <param name="direction">Direction of the sweep (does not need to be normalized).</param>
    /// <param name="maxDistance">Maximum distance the sphere can travel.</param>
    /// <param name="hit">Information about the closest hit, if the method returns <c>true</c>.</param>
    /// <param name="raycastParams">Optional filtering options.</param>
    /// <returns><c>true</c> if the sphere hit something within <paramref name="maxDistance"/>.</returns>
    public static bool SphereCast(Vector3 origin, float radius, Vector3 direction, float maxDistance, out ShapeCastHit hit, RaycastParams? raycastParams = null)
    {
        hit = default;
        var physics = CurrentScene?.PhysicsSystem;
        if (physics == null)
            return false;

        var shape = new Sphere(MathF.Max(0.001f, radius));
        var pose = new RigidPose(origin);
        return physics.SweepClosest(shape, pose, direction, maxDistance, out hit, raycastParams ?? default);
    }

    /// <summary>
    /// Sweeps a sphere along a direction and returns all hits.
    /// </summary>
    /// <param name="origin">World-space center of the sphere at the start of the sweep.</param>
    /// <param name="radius">Radius of the sphere.</param>
    /// <param name="direction">Direction of the sweep (does not need to be normalized).</param>
    /// <param name="maxDistance">Maximum distance the sphere can travel.</param>
    /// <param name="raycastParams">Optional filtering options.</param>
    /// <returns>A list of all hits along the sweep, unordered.</returns>
    public static List<ShapeCastHit> SphereCastAll(Vector3 origin, float radius, Vector3 direction, float maxDistance, RaycastParams? raycastParams = null)
    {
        var physics = CurrentScene?.PhysicsSystem;
        if (physics == null)
            return new List<ShapeCastHit>();

        var shape = new Sphere(MathF.Max(0.001f, radius));
        var pose = new RigidPose(origin);
        return physics.SweepAll(shape, pose, direction, maxDistance, raycastParams ?? default);
    }

    // --- Box Cast ---

    /// <summary>
    /// Sweeps a box along a direction and returns the closest hit, if any.
    /// </summary>
    /// <param name="origin">World-space center of the box at the start of the sweep.</param>
    /// <param name="halfExtents">Half-size of the box on each axis.</param>
    /// <param name="orientation">Rotation of the box.</param>
    /// <param name="direction">Direction of the sweep (does not need to be normalized).</param>
    /// <param name="maxDistance">Maximum distance the box can travel.</param>
    /// <param name="hit">Information about the closest hit, if the method returns <c>true</c>.</param>
    /// <param name="raycastParams">Optional filtering options.</param>
    /// <returns><c>true</c> if the box hit something within <paramref name="maxDistance"/>.</returns>
    public static bool BoxCast(Vector3 origin, Vector3 halfExtents, Quaternion orientation, Vector3 direction, float maxDistance, out ShapeCastHit hit, RaycastParams? raycastParams = null)
    {
        hit = default;
        var physics = CurrentScene?.PhysicsSystem;
        if (physics == null)
            return false;

        var shape = new Box(
            MathF.Max(0.001f, halfExtents.X * 2f),
            MathF.Max(0.001f, halfExtents.Y * 2f),
            MathF.Max(0.001f, halfExtents.Z * 2f));
        var pose = new RigidPose(origin, orientation);
        return physics.SweepClosest(shape, pose, direction, maxDistance, out hit, raycastParams ?? default);
    }

    /// <summary>
    /// Sweeps a box along a direction and returns all hits.
    /// </summary>
    /// <param name="origin">World-space center of the box at the start of the sweep.</param>
    /// <param name="halfExtents">Half-size of the box on each axis.</param>
    /// <param name="orientation">Rotation of the box.</param>
    /// <param name="direction">Direction of the sweep (does not need to be normalized).</param>
    /// <param name="maxDistance">Maximum distance the box can travel.</param>
    /// <param name="raycastParams">Optional filtering options.</param>
    /// <returns>A list of all hits along the sweep, unordered.</returns>
    public static List<ShapeCastHit> BoxCastAll(Vector3 origin, Vector3 halfExtents, Quaternion orientation, Vector3 direction, float maxDistance, RaycastParams? raycastParams = null)
    {
        var physics = CurrentScene?.PhysicsSystem;
        if (physics == null)
            return new List<ShapeCastHit>();

        var shape = new Box(
            MathF.Max(0.001f, halfExtents.X * 2f),
            MathF.Max(0.001f, halfExtents.Y * 2f),
            MathF.Max(0.001f, halfExtents.Z * 2f));
        var pose = new RigidPose(origin, orientation);
        return physics.SweepAll(shape, pose, direction, maxDistance, raycastParams ?? default);
    }

    // --- Capsule Cast ---

    /// <summary>
    /// Sweeps a capsule along a direction and returns the closest hit, if any.
    /// </summary>
    /// <param name="origin">World-space center of the capsule at the start of the sweep.</param>
    /// <param name="radius">Radius of the capsule.</param>
    /// <param name="length">Length of the capsule's cylindrical segment (total height = length + 2 * radius).</param>
    /// <param name="orientation">Rotation of the capsule.</param>
    /// <param name="direction">Direction of the sweep (does not need to be normalized).</param>
    /// <param name="maxDistance">Maximum distance the capsule can travel.</param>
    /// <param name="hit">Information about the closest hit, if the method returns <c>true</c>.</param>
    /// <param name="raycastParams">Optional filtering options.</param>
    /// <returns><c>true</c> if the capsule hit something within <paramref name="maxDistance"/>.</returns>
    public static bool CapsuleCast(Vector3 origin, float radius, float length, Quaternion orientation, Vector3 direction, float maxDistance, out ShapeCastHit hit, RaycastParams? raycastParams = null)
    {
        hit = default;
        var physics = CurrentScene?.PhysicsSystem;
        if (physics == null)
            return false;

        var shape = new Capsule(MathF.Max(0.001f, radius), MathF.Max(0.001f, length));
        var pose = new RigidPose(origin, orientation);
        return physics.SweepClosest(shape, pose, direction, maxDistance, out hit, raycastParams ?? default);
    }

    /// <summary>
    /// Sweeps a capsule along a direction and returns all hits.
    /// </summary>
    /// <param name="origin">World-space center of the capsule at the start of the sweep.</param>
    /// <param name="radius">Radius of the capsule.</param>
    /// <param name="length">Length of the capsule's cylindrical segment (total height = length + 2 * radius).</param>
    /// <param name="orientation">Rotation of the capsule.</param>
    /// <param name="direction">Direction of the sweep (does not need to be normalized).</param>
    /// <param name="maxDistance">Maximum distance the capsule can travel.</param>
    /// <param name="raycastParams">Optional filtering options.</param>
    /// <returns>A list of all hits along the sweep, unordered.</returns>
    public static List<ShapeCastHit> CapsuleCastAll(Vector3 origin, float radius, float length, Quaternion orientation, Vector3 direction, float maxDistance, RaycastParams? raycastParams = null)
    {
        var physics = CurrentScene?.PhysicsSystem;
        if (physics == null)
            return new List<ShapeCastHit>();

        var shape = new Capsule(MathF.Max(0.001f, radius), MathF.Max(0.001f, length));
        var pose = new RigidPose(origin, orientation);
        return physics.SweepAll(shape, pose, direction, maxDistance, raycastParams ?? default);
    }

    // --- Overlap Queries ---

    /// <summary>
    /// Finds all entities whose colliders overlap a sphere at the given position.
    /// </summary>
    /// <param name="center">World-space center of the overlap sphere.</param>
    /// <param name="radius">Radius of the sphere.</param>
    /// <param name="raycastParams">Optional filtering options.</param>
    /// <returns>A list of entities whose colliders overlap the sphere.</returns>
    public static List<Entity> OverlapSphere(Vector3 center, float radius, RaycastParams? raycastParams = null)
    {
        var physics = CurrentScene?.PhysicsSystem;
        if (physics == null)
            return new List<Entity>();

        var shape = new Sphere(MathF.Max(0.001f, radius));
        var pose = new RigidPose(center);
        return physics.OverlapQuery(shape, pose, raycastParams ?? default);
    }

    /// <summary>
    /// Finds all entities whose colliders overlap a box at the given position and orientation.
    /// </summary>
    /// <param name="center">World-space center of the overlap box.</param>
    /// <param name="halfExtents">Half-size of the box on each axis.</param>
    /// <param name="orientation">Rotation of the box.</param>
    /// <param name="raycastParams">Optional filtering options.</param>
    /// <returns>A list of entities whose colliders overlap the box.</returns>
    public static List<Entity> OverlapBox(Vector3 center, Vector3 halfExtents, Quaternion orientation, RaycastParams? raycastParams = null)
    {
        var physics = CurrentScene?.PhysicsSystem;
        if (physics == null)
            return new List<Entity>();

        var shape = new Box(
            MathF.Max(0.001f, halfExtents.X * 2f),
            MathF.Max(0.001f, halfExtents.Y * 2f),
            MathF.Max(0.001f, halfExtents.Z * 2f));
        var pose = new RigidPose(center, orientation);
        return physics.OverlapQuery(shape, pose, raycastParams ?? default);
    }

    /// <summary>
    /// Finds all entities whose colliders overlap a capsule at the given position and orientation.
    /// </summary>
    /// <param name="center">World-space center of the overlap capsule.</param>
    /// <param name="radius">Radius of the capsule.</param>
    /// <param name="length">Length of the capsule's cylindrical segment.</param>
    /// <param name="orientation">Rotation of the capsule.</param>
    /// <param name="raycastParams">Optional filtering options.</param>
    /// <returns>A list of entities whose colliders overlap the capsule.</returns>
    public static List<Entity> OverlapCapsule(Vector3 center, float radius, float length, Quaternion orientation, RaycastParams? raycastParams = null)
    {
        var physics = CurrentScene?.PhysicsSystem;
        if (physics == null)
            return new List<Entity>();

        var shape = new Capsule(MathF.Max(0.001f, radius), MathF.Max(0.001f, length));
        var pose = new RigidPose(center, orientation);
        return physics.OverlapQuery(shape, pose, raycastParams ?? default);
    }
}
