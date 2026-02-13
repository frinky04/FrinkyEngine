using System.Numerics;
using FrinkyEngine.Core.ECS;
using Raylib_cs;

namespace FrinkyEngine.Core.Animation.IK;

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
    public int RootBoneIndex { get; set; }

    /// <summary>
    /// Mid bone index (e.g. forearm / shin). Dropdown index where 0 = (none).
    /// </summary>
    [InspectorDropdown(nameof(GetBoneNames))]
    public int MidBoneIndex { get; set; }

    /// <summary>
    /// End bone index (e.g. hand / foot). Dropdown index where 0 = (none).
    /// </summary>
    [InspectorDropdown(nameof(GetBoneNames))]
    public int EndBoneIndex { get; set; }

    /// <summary>
    /// World-space target position the end effector should reach toward.
    /// </summary>
    public Vector3 TargetPosition { get; set; }

    /// <summary>
    /// World-space pole target that defines the bend plane (e.g. knee/elbow direction).
    /// </summary>
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

        if (root < 0 || mid < 0 || end < 0)
            return false;
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

        // Dropdown index 0 = (none), so actual bone index = dropdown - 1
        int root = RootBoneIndex - 1;
        int mid = MidBoneIndex - 1;
        int end = EndBoneIndex - 1;

        // Current world-space positions
        var rootPos = IKMath.ExtractTranslation(worldMatrices[root]);
        var midPos = IKMath.ExtractTranslation(worldMatrices[mid]);
        var endPos = IKMath.ExtractTranslation(worldMatrices[end]);

        float upperLen = Vector3.Distance(rootPos, midPos);
        float lowerLen = Vector3.Distance(midPos, endPos);

        // Compute where the mid joint should be
        var newMidPos = IKMath.ComputeTwoBoneMidPosition(rootPos, upperLen, lowerLen, TargetPosition, PoleTargetPosition);
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

        ApplyWorldRotationDelta(localTransforms, worldMatrices, hierarchy, root, rootDelta);

        // Recompute FK from root onward so mid/end positions update
        IKMath.ForwardKinematics(hierarchy.ParentIndices, localTransforms, entityWorldMatrix, worldMatrices);

        // --- Mid bone rotation ---
        // After root rotation, get the new actual positions
        var updatedMidPos = IKMath.ExtractTranslation(worldMatrices[mid]);
        var updatedEndPos = IKMath.ExtractTranslation(worldMatrices[end]);

        var oldLowerVec = updatedEndPos - updatedMidPos;
        var desiredLowerVec = TargetPosition - updatedMidPos;
        if (oldLowerVec.LengthSquared() < 1e-8f || desiredLowerVec.LengthSquared() < 1e-8f)
            return;

        var oldLowerDir = Vector3.Normalize(oldLowerVec);
        var desiredLowerDir = Vector3.Normalize(desiredLowerVec);
        var midDelta = IKMath.RotationBetween(oldLowerDir, desiredLowerDir);

        if (Weight < 1f)
            midDelta = Quaternion.Slerp(Quaternion.Identity, midDelta, Weight);

        ApplyWorldRotationDelta(localTransforms, worldMatrices, hierarchy, mid, midDelta);
        IKMath.ForwardKinematics(hierarchy.ParentIndices, localTransforms, entityWorldMatrix, worldMatrices);
    }

    private static void ApplyWorldRotationDelta(
        (Vector3 translation, Quaternion rotation, Vector3 scale)[] localTransforms,
        Matrix4x4[] worldMatrices,
        BoneHierarchy hierarchy,
        int boneIndex,
        Quaternion worldDelta)
    {
        // Current world rotation of this bone
        var currentWorldRot = IKMath.ExtractRotation(worldMatrices[boneIndex]);
        // Apply the delta in world space
        var newWorldRot = Quaternion.Normalize(Raymath.QuaternionMultiply(worldDelta, currentWorldRot));

        // Convert new world rotation to local space
        int parentIdx = hierarchy.ParentIndices[boneIndex];
        Quaternion newLocalRot;
        if (parentIdx >= 0 && parentIdx < worldMatrices.Length)
            newLocalRot = IKMath.WorldToLocalRotation(newWorldRot, worldMatrices[parentIdx]);
        else
            newLocalRot = newWorldRot; // root bone — world == local

        var (t, _, s) = localTransforms[boneIndex];
        localTransforms[boneIndex] = (t, Quaternion.Normalize(newLocalRot), s);
    }

    private string[] GetBoneNames()
    {
        return Hierarchy?.GetBoneNamesForDropdown() ?? ["(none)"];
    }
}
