using System.Numerics;
using FrinkyEngine.Core.ECS;

namespace FrinkyEngine.Core.Physics;

/// <summary>
/// Contains information about a collision between two physics bodies.
/// </summary>
public readonly struct CollisionInfo
{
    /// <summary>
    /// The other entity involved in the collision.
    /// </summary>
    public Entity Other { get; init; }

    /// <summary>
    /// World-space contact point.
    /// </summary>
    public Vector3 ContactPoint { get; init; }

    /// <summary>
    /// Contact normal pointing from the other entity toward this entity.
    /// </summary>
    public Vector3 Normal { get; init; }

    /// <summary>
    /// Penetration depth of the collision contact.
    /// </summary>
    public float PenetrationDepth { get; init; }
}
