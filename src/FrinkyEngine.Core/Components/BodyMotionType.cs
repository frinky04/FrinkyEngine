namespace FrinkyEngine.Core.Components;

/// <summary>
/// Controls how a rigidbody is simulated.
/// </summary>
public enum BodyMotionType
{
    /// <summary>
    /// Fully simulated by physics.
    /// </summary>
    Dynamic,

    /// <summary>
    /// Moved by gameplay code, collides with dynamics.
    /// </summary>
    Kinematic,

    /// <summary>
    /// Immovable collision geometry.
    /// </summary>
    Static
}

