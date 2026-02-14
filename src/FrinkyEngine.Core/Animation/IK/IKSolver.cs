using System.Numerics;
using FrinkyEngine.Core.ECS;

namespace FrinkyEngine.Core.Animation.IK;

/// <summary>
/// Abstract base class for IK solvers. Each solver modifies bone-local transforms in place.
/// </summary>
public abstract class IKSolver : FObject
{
    /// <summary>
    /// Whether this solver is active.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Blend weight for this solver (0 = no effect, 1 = full IK).
    /// </summary>
    [InspectorRange(0f, 1f, 0.01f)]
    public float Weight { get; set; } = 1f;

    /// <summary>
    /// Bone hierarchy reference set by the owning component. Used for dropdown support and solving.
    /// </summary>
    internal BoneHierarchy? Hierarchy { get; set; }

    /// <summary>
    /// Returns bone names for inspector dropdowns (index 0 = "(none)").
    /// </summary>
    protected string[] GetBoneNames()
    {
        return Hierarchy?.GetBoneNamesForDropdown() ?? ["(none)"];
    }

    /// <summary>
    /// Whether the solver has enough configuration data to run.
    /// </summary>
    public virtual bool IsConfigured => true;

    /// <summary>
    /// Whether the solver can run on the provided hierarchy.
    /// </summary>
    public virtual bool CanSolve(BoneHierarchy hierarchy) => IsConfigured;

    /// <summary>
    /// Applies this solver to the given local-space bone transforms.
    /// </summary>
    /// <param name="localTransforms">Per-bone local transforms (translation, rotation, scale).</param>
    /// <param name="hierarchy">Bone hierarchy data.</param>
    /// <param name="entityWorldMatrix">The entity's world transform matrix.</param>
    /// <param name="worldMatrices">Pre-computed world-space matrices for all bones (FK result). Solvers may read and mutate this.</param>
    public abstract void Solve(
        (Vector3 translation, Quaternion rotation, Vector3 scale)[] localTransforms,
        BoneHierarchy hierarchy,
        Matrix4x4 entityWorldMatrix,
        Matrix4x4[] worldMatrices);
}
