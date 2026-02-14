using System.Numerics;
using FrinkyEngine.Core.ECS;

namespace FrinkyEngine.Core.Animation.IK;

/// <summary>
/// Coordinate space for IK targets.
/// </summary>
public enum IKTargetSpace
{
    /// <summary>
    /// Target is specified in world space.
    /// </summary>
    World,

    /// <summary>
    /// Target is specified relative to the owning entity's transform.
    /// </summary>
    Local
}

/// <summary>
/// Two-bone (3-joint) IK solver for limb chains such as arms and legs.
/// </summary>
public class TwoBoneIKSolver : IKSolver
{
    /// <inheritdoc/>
    public override string DisplayName => "Two Bone IK";

    /// <summary>
    /// Root bone index (e.g. upper arm / thigh). Dropdown index where 0 = (none).
    /// </summary>
    [InspectorDropdown(nameof(GetBoneNames))]
    [InspectorLabel("Root Bone")]
    public int RootBoneIndex { get; set; }

    /// <summary>
    /// Mid bone index (e.g. forearm / shin). Dropdown index where 0 = (none).
    /// </summary>
    [InspectorDropdown(nameof(GetBoneNames))]
    [InspectorLabel("Mid Bone")]
    public int MidBoneIndex { get; set; }

    /// <summary>
    /// End bone index (e.g. hand / foot). Dropdown index where 0 = (none).
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
    /// Interpreted according to <see cref="TargetSpace"/>.
    /// </summary>
    [InspectorGizmo("Target", ColorR = 50, ColorG = 255, ColorB = 50, SpaceProperty = nameof(TargetSpace))]
    public Vector3 TargetPosition { get; set; }

    /// <summary>
    /// Coordinate space for <see cref="PoleTargetPosition"/>.
    /// </summary>
    [InspectorSection("Pole Target")]
    public IKTargetSpace PoleTargetSpace { get; set; } = IKTargetSpace.World;

    /// <summary>
    /// Pole target that defines the bend plane (e.g. knee/elbow direction).
    /// Interpreted according to <see cref="PoleTargetSpace"/>.
    /// </summary>
    [InspectorGizmo("Pole", ColorR = 255, ColorG = 100, ColorB = 50, SpaceProperty = nameof(PoleTargetSpace))]
    public Vector3 PoleTargetPosition { get; set; }

    /// <inheritdoc/>
    public override bool IsConfigured =>
        RootBoneIndex > 0
        && MidBoneIndex > 0
        && EndBoneIndex > 0
        && RootBoneIndex != MidBoneIndex
        && RootBoneIndex != EndBoneIndex
        && MidBoneIndex != EndBoneIndex;

    /// <inheritdoc/>
    public override bool CanSolve(BoneHierarchy hierarchy)
    {
        if (!IsConfigured)
            return false;

        int root = RootBoneIndex - 1;
        int mid = MidBoneIndex - 1;
        int end = EndBoneIndex - 1;

        if (root >= hierarchy.BoneCount || mid >= hierarchy.BoneCount || end >= hierarchy.BoneCount)
            return false;

        return hierarchy.ParentIndices[mid] == root
            && hierarchy.ParentIndices[end] == mid;
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

        // Resolve targets to world space
        var worldTarget = TargetSpace == IKTargetSpace.Local
            ? Vector3.Transform(TargetPosition, entityWorldMatrix)
            : TargetPosition;

        var worldPole = PoleTargetSpace == IKTargetSpace.Local
            ? Vector3.Transform(PoleTargetPosition, entityWorldMatrix)
            : PoleTargetPosition;

        // Dropdown index 0 = (none), so actual bone index = dropdown - 1
        int root = RootBoneIndex - 1;
        int mid = MidBoneIndex - 1;
        int end = EndBoneIndex - 1;

        // Current world-space positions
        var rootPos = worldMatrices[root].Translation;
        var midPos = worldMatrices[mid].Translation;
        var endPos = worldMatrices[end].Translation;

        float upperLen = Vector3.Distance(rootPos, midPos);
        float lowerLen = Vector3.Distance(midPos, endPos);

        // Compute where the mid joint should be
        var newMidPos = IKMath.ComputeTwoBoneMidPosition(rootPos, upperLen, lowerLen, worldTarget, worldPole);
        if (newMidPos == null)
            return;

        // --- Root bone rotation ---
        // Current upper-limb direction vs desired
        var oldUpperVec = midPos - rootPos;
        var newUpperVec = newMidPos.Value - rootPos;
        if (oldUpperVec.LengthSquared() < 1e-8f || newUpperVec.LengthSquared() < 1e-8f)
            return;

        var oldUpperDir = Vector3.Normalize(oldUpperVec);
        var newUpperDir = Vector3.Normalize(newUpperVec);
        var rootDelta = IKMath.RotationBetween(oldUpperDir, newUpperDir);

        if (Weight < 1f)
            rootDelta = Quaternion.Slerp(Quaternion.Identity, rootDelta, Weight);

        IKMath.ApplyWorldRotationDelta(localTransforms, worldMatrices, hierarchy, root, rootDelta);

        // Recompute FK from root onward so mid/end positions update
        IKMath.ForwardKinematics(hierarchy.ParentIndices, localTransforms, entityWorldMatrix, worldMatrices);

        // --- Mid bone rotation ---
        // After root rotation, get the new actual positions
        var updatedMidPos = worldMatrices[mid].Translation;
        var updatedEndPos = worldMatrices[end].Translation;

        var oldLowerVec = updatedEndPos - updatedMidPos;
        var desiredLowerVec = worldTarget - updatedMidPos;
        if (oldLowerVec.LengthSquared() < 1e-8f || desiredLowerVec.LengthSquared() < 1e-8f)
            return;

        var oldLowerDir = Vector3.Normalize(oldLowerVec);
        var desiredLowerDir = Vector3.Normalize(desiredLowerVec);
        var midDelta = IKMath.RotationBetween(oldLowerDir, desiredLowerDir);

        if (Weight < 1f)
            midDelta = Quaternion.Slerp(Quaternion.Identity, midDelta, Weight);

        IKMath.ApplyWorldRotationDelta(localTransforms, worldMatrices, hierarchy, mid, midDelta);
        IKMath.ForwardKinematics(hierarchy.ParentIndices, localTransforms, entityWorldMatrix, worldMatrices);
    }
}
