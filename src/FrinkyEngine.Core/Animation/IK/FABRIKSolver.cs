using System.Numerics;
using FrinkyEngine.Core.ECS;

namespace FrinkyEngine.Core.Animation.IK;

/// <summary>
/// FABRIK (Forward And Backward Reaching Inverse Kinematics) solver for arbitrary-length bone chains.
/// </summary>
public class FABRIKSolver : IKSolver
{
    /// <inheritdoc/>
    public override string DisplayName => "FABRIK";

    /// <summary>
    /// Root bone of the chain. Dropdown index where 0 = (none).
    /// </summary>
    [InspectorDropdown(nameof(GetBoneNames))]
    [InspectorLabel("Root Bone")]
    public int RootBoneIndex { get; set; }

    /// <summary>
    /// End effector bone. Dropdown index where 0 = (none).
    /// </summary>
    [InspectorDropdown(nameof(GetBoneNames))]
    [InspectorLabel("End Bone")]
    public int EndBoneIndex { get; set; }

    /// <summary>
    /// Coordinate space for <see cref="TargetPosition"/>.
    /// </summary>
    [InspectorSection("Target")]
    public IKTargetSpace TargetSpace { get; set; } = IKTargetSpace.World;

    /// <summary>
    /// Target position the end effector should reach toward.
    /// </summary>
    [InspectorGizmo("Target", ColorR = 50, ColorG = 255, ColorB = 50, SpaceProperty = nameof(TargetSpace))]
    public Vector3 TargetPosition { get; set; }

    /// <summary>
    /// Maximum number of FABRIK iterations per solve.
    /// </summary>
    [InspectorSection("Solver")]
    [InspectorRange(1, 50, 1)]
    [InspectorLabel("Max Iterations")]
    public int MaxIterations { get; set; } = 10;

    /// <summary>
    /// Convergence threshold in world units. Iteration stops when the end effector is within this distance of the target.
    /// </summary>
    [InspectorRange(0.0001f, 0.1f, 0.0001f)]
    public float Tolerance { get; set; } = 0.001f;

    /// <inheritdoc/>
    public override bool IsConfigured =>
        RootBoneIndex > 0
        && EndBoneIndex > 0
        && RootBoneIndex != EndBoneIndex;

    /// <inheritdoc/>
    public override bool CanSolve(BoneHierarchy hierarchy)
    {
        if (!IsConfigured)
            return false;

        int root = RootBoneIndex - 1;
        int end = EndBoneIndex - 1;

        if (root >= hierarchy.BoneCount || end >= hierarchy.BoneCount)
            return false;

        return hierarchy.GetChain(root, end) != null;
    }

    /// <inheritdoc/>
    public override void Solve(
        (Vector3 translation, Quaternion rotation, Vector3 scale)[] localTransforms,
        BoneHierarchy hierarchy,
        Matrix4x4 entityWorldMatrix,
        Matrix4x4[] worldMatrices)
    {
        if (!Enabled || Weight <= 0f || !CanSolve(hierarchy))
            return;

        int root = RootBoneIndex - 1;
        int end = EndBoneIndex - 1;

        var chain = hierarchy.GetChain(root, end);
        if (chain == null || chain.Length < 2)
            return;

        var worldTarget = TargetSpace == IKTargetSpace.Local
            ? Vector3.Transform(TargetPosition, entityWorldMatrix)
            : TargetPosition;

        int n = chain.Length;

        // Extract world positions for each joint in the chain
        var positions = new Vector3[n];
        for (int i = 0; i < n; i++)
            positions[i] = worldMatrices[chain[i]].Translation;

        // Compute segment lengths
        var lengths = new float[n - 1];
        float totalLength = 0f;
        for (int i = 0; i < n - 1; i++)
        {
            lengths[i] = Vector3.Distance(positions[i], positions[i + 1]);
            totalLength += lengths[i];
        }

        if (totalLength < 1e-6f)
            return;

        var rootPos = positions[0];
        float distToTarget = Vector3.Distance(rootPos, worldTarget);

        // Unreachable: stretch chain toward target
        if (distToTarget > totalLength)
        {
            var dir = Vector3.Normalize(worldTarget - rootPos);
            for (int i = 1; i < n; i++)
                positions[i] = positions[i - 1] + dir * lengths[i - 1];
        }
        else
        {
            // FABRIK iteration
            for (int iter = 0; iter < MaxIterations; iter++)
            {
                // Check convergence
                if (Vector3.Distance(positions[n - 1], worldTarget) <= Tolerance)
                    break;

                // Backward pass: end → root
                positions[n - 1] = worldTarget;
                for (int i = n - 2; i >= 0; i--)
                {
                    var dir = positions[i] - positions[i + 1];
                    float len = dir.Length();
                    if (len < 1e-8f)
                        dir = Vector3.UnitY;
                    else
                        dir /= len;
                    positions[i] = positions[i + 1] + dir * lengths[i];
                }

                // Forward pass: root → end
                positions[0] = rootPos;
                for (int i = 1; i < n; i++)
                {
                    var dir = positions[i] - positions[i - 1];
                    float len = dir.Length();
                    if (len < 1e-8f)
                        dir = Vector3.UnitY;
                    else
                        dir /= len;
                    positions[i] = positions[i - 1] + dir * lengths[i - 1];
                }
            }
        }

        // Convert position changes to rotation deltas and apply
        for (int i = 0; i < n - 1; i++)
        {
            int boneIdx = chain[i];
            int childIdx = chain[i + 1];

            var oldChildDir = worldMatrices[childIdx].Translation - worldMatrices[boneIdx].Translation;
            var newChildDir = positions[i + 1] - positions[i];

            if (oldChildDir.LengthSquared() < 1e-8f || newChildDir.LengthSquared() < 1e-8f)
                continue;

            var delta = IKMath.RotationBetween(
                Vector3.Normalize(oldChildDir),
                Vector3.Normalize(newChildDir));

            if (Weight < 1f)
                delta = Quaternion.Slerp(Quaternion.Identity, delta, Weight);

            IKMath.ApplyWorldRotationDelta(localTransforms, worldMatrices, hierarchy, boneIdx, delta);
            IKMath.ForwardKinematics(hierarchy.ParentIndices, localTransforms, entityWorldMatrix, worldMatrices);
        }
    }
}
