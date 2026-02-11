using System.Numerics;
using FrinkyEngine.Core.ECS;

namespace FrinkyEngine.Core.Physics;

/// <summary>
/// Contains information about a single raycast hit against a physics collider.
/// </summary>
public readonly struct RaycastHit
{
    /// <summary>
    /// The entity whose collider was hit.
    /// </summary>
    public Entity Entity { get; init; }

    /// <summary>
    /// World-space point of the ray impact.
    /// </summary>
    public Vector3 Point { get; init; }

    /// <summary>
    /// Surface normal at the hit location.
    /// </summary>
    public Vector3 Normal { get; init; }

    /// <summary>
    /// Distance from the ray origin to the hit point.
    /// </summary>
    public float Distance { get; init; }
}
