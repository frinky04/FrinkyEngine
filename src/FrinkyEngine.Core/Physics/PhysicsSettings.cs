using System.Numerics;

namespace FrinkyEngine.Core.Physics;

/// <summary>
/// Scene-level physics configuration (gravity only; other physics settings live in PhysicsProjectSettings).
/// </summary>
public class PhysicsSettings
{
    /// <summary>
    /// Gravity acceleration applied to dynamic rigidbodies.
    /// </summary>
    public Vector3 Gravity { get; set; } = new(0f, -9.81f, 0f);

    /// <summary>
    /// Ensures all settings remain in safe ranges.
    /// </summary>
    public void Normalize()
    {
        // Gravity is free-form; no clamping needed.
    }

    /// <summary>
    /// Returns a deep copy of this settings object.
    /// </summary>
    public PhysicsSettings Clone()
    {
        return new PhysicsSettings
        {
            Gravity = Gravity
        };
    }
}
