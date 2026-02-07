using System.Numerics;

namespace FrinkyEngine.Core.Physics;

/// <summary>
/// Scene-level physics configuration.
/// </summary>
public class PhysicsSettings
{
    /// <summary>
    /// Gravity acceleration applied to dynamic rigidbodies.
    /// </summary>
    public Vector3 Gravity { get; set; } = new(0f, -9.81f, 0f);

    /// <summary>
    /// Fixed simulation step duration, in seconds.
    /// </summary>
    public float FixedTimestep { get; set; } = 1f / 60f;

    /// <summary>
    /// Maximum number of simulation steps allowed for one frame.
    /// </summary>
    public int MaxSubstepsPerFrame { get; set; } = 4;

    /// <summary>
    /// Solver velocity iterations per substep.
    /// </summary>
    public int SolverVelocityIterations { get; set; } = 8;

    /// <summary>
    /// Solver substep count.
    /// </summary>
    public int SolverSubsteps { get; set; } = 1;

    /// <summary>
    /// Contact spring angular frequency.
    /// </summary>
    public float ContactSpringFrequency { get; set; } = 30f;

    /// <summary>
    /// Contact spring damping ratio.
    /// </summary>
    public float ContactDampingRatio { get; set; } = 1f;

    /// <summary>
    /// Recovery velocity cap before restitution scaling.
    /// </summary>
    public float MaximumRecoveryVelocity { get; set; } = 2f;

    /// <summary>
    /// Default friction for colliders without overrides.
    /// </summary>
    public float DefaultFriction { get; set; } = 0.8f;

    /// <summary>
    /// Default restitution for colliders without overrides.
    /// </summary>
    public float DefaultRestitution { get; set; } = 0f;

    /// <summary>
    /// Ensures all settings remain in safe ranges.
    /// </summary>
    public void Normalize()
    {
        FixedTimestep = float.IsFinite(FixedTimestep) ? Math.Clamp(FixedTimestep, 1f / 240f, 1f / 15f) : 1f / 60f;
        MaxSubstepsPerFrame = Math.Clamp(MaxSubstepsPerFrame, 1, 16);
        SolverVelocityIterations = Math.Clamp(SolverVelocityIterations, 1, 32);
        SolverSubsteps = Math.Clamp(SolverSubsteps, 1, 8);
        ContactSpringFrequency = float.IsFinite(ContactSpringFrequency)
            ? Math.Clamp(ContactSpringFrequency, 1f, 300f)
            : 30f;
        ContactDampingRatio = float.IsFinite(ContactDampingRatio)
            ? Math.Clamp(ContactDampingRatio, 0f, 10f)
            : 1f;
        MaximumRecoveryVelocity = float.IsFinite(MaximumRecoveryVelocity)
            ? Math.Clamp(MaximumRecoveryVelocity, 0f, 100f)
            : 2f;
        DefaultFriction = float.IsFinite(DefaultFriction)
            ? Math.Clamp(DefaultFriction, 0f, 10f)
            : 0.8f;
        DefaultRestitution = float.IsFinite(DefaultRestitution)
            ? Math.Clamp(DefaultRestitution, 0f, 1f)
            : 0f;
    }

    /// <summary>
    /// Returns a deep copy of this settings object.
    /// </summary>
    public PhysicsSettings Clone()
    {
        return new PhysicsSettings
        {
            Gravity = Gravity,
            FixedTimestep = FixedTimestep,
            MaxSubstepsPerFrame = MaxSubstepsPerFrame,
            SolverVelocityIterations = SolverVelocityIterations,
            SolverSubsteps = SolverSubsteps,
            ContactSpringFrequency = ContactSpringFrequency,
            ContactDampingRatio = ContactDampingRatio,
            MaximumRecoveryVelocity = MaximumRecoveryVelocity,
            DefaultFriction = DefaultFriction,
            DefaultRestitution = DefaultRestitution
        };
    }
}

