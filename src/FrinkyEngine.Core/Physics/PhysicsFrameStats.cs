namespace FrinkyEngine.Core.Physics;

/// <summary>
/// Per-frame physics diagnostics snapshot.
/// </summary>
public readonly record struct PhysicsFrameStats(
    bool Valid,
    int DynamicBodies,
    int KinematicBodies,
    int StaticBodies,
    int SubstepsThisFrame,
    double StepTimeMs,
    int ActiveCharacterControllers);
