namespace FrinkyEngine.Core.Physics;

/// <summary>
/// Project-level physics settings (shared across all scenes).
/// Populated from <see cref="Assets.RuntimeProjectSettings"/> at startup.
/// </summary>
public class PhysicsProjectSettings
{
    /// <summary>
    /// Global singleton instance, set by the editor or runtime at startup.
    /// </summary>
    public static PhysicsProjectSettings Current { get; set; } = new();

    /// <summary>Fixed simulation step duration, in seconds.</summary>
    public float FixedTimestep { get; set; } = 1f / 60f;
    /// <summary>Maximum number of simulation steps allowed for one frame.</summary>
    public int MaxSubstepsPerFrame { get; set; } = 4;
    /// <summary>Solver velocity iterations per substep.</summary>
    public int SolverVelocityIterations { get; set; } = 8;
    /// <summary>Solver substep count.</summary>
    public int SolverSubsteps { get; set; } = 1;
    /// <summary>Contact spring angular frequency.</summary>
    public float ContactSpringFrequency { get; set; } = 30f;
    /// <summary>Contact spring damping ratio.</summary>
    public float ContactDampingRatio { get; set; } = 1f;
    /// <summary>Recovery velocity cap before restitution scaling.</summary>
    public float MaximumRecoveryVelocity { get; set; } = 2f;
    /// <summary>Default friction for colliders without overrides.</summary>
    public float DefaultFriction { get; set; } = 0.8f;
    /// <summary>Default restitution for colliders without overrides.</summary>
    public float DefaultRestitution { get; set; } = 0f;

    /// <summary>
    /// Populates <see cref="Current"/> from the given runtime project settings.
    /// </summary>
    public static void ApplyFrom(Assets.RuntimeProjectSettings runtime)
    {
        Current = new PhysicsProjectSettings
        {
            FixedTimestep = runtime.PhysicsFixedTimestep,
            MaxSubstepsPerFrame = runtime.PhysicsMaxSubstepsPerFrame,
            SolverVelocityIterations = runtime.PhysicsSolverVelocityIterations,
            SolverSubsteps = runtime.PhysicsSolverSubsteps,
            ContactSpringFrequency = runtime.PhysicsContactSpringFrequency,
            ContactDampingRatio = runtime.PhysicsContactDampingRatio,
            MaximumRecoveryVelocity = runtime.PhysicsMaximumRecoveryVelocity,
            DefaultFriction = runtime.PhysicsDefaultFriction,
            DefaultRestitution = runtime.PhysicsDefaultRestitution
        };
        Current.Normalize();
    }

    /// <summary>
    /// Populates <see cref="Current"/> from an export manifest's physics settings.
    /// </summary>
    public static void ApplyFrom(Assets.ExportManifest manifest)
    {
        Current = new PhysicsProjectSettings
        {
            FixedTimestep = manifest.PhysicsFixedTimestep ?? 1f / 60f,
            MaxSubstepsPerFrame = manifest.PhysicsMaxSubstepsPerFrame ?? 4,
            SolverVelocityIterations = manifest.PhysicsSolverVelocityIterations ?? 8,
            SolverSubsteps = manifest.PhysicsSolverSubsteps ?? 1,
            ContactSpringFrequency = manifest.PhysicsContactSpringFrequency ?? 30f,
            ContactDampingRatio = manifest.PhysicsContactDampingRatio ?? 1f,
            MaximumRecoveryVelocity = manifest.PhysicsMaximumRecoveryVelocity ?? 2f,
            DefaultFriction = manifest.PhysicsDefaultFriction ?? 0.8f,
            DefaultRestitution = manifest.PhysicsDefaultRestitution ?? 0f
        };
        Current.Normalize();
    }

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
}
