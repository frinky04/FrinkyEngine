using System.Numerics;
using FrinkyEngine.Core.ECS;
using Raylib_cs;

namespace FrinkyEngine.Core.Animation.IK;

/// <summary>
/// Which local axis to use for aim/up alignment.
/// </summary>
public enum LocalAxis
{
    XPositive,
    YPositive,
    ZPositive,
    XNegative,
    YNegative,
    ZNegative
}

/// <summary>
/// Rotates a single bone so that a chosen local axis points toward a target position.
/// Useful for head tracking, turrets, eye gaze, etc.
/// </summary>
public class LookAtIKSolver : IKSolver
{
    /// <inheritdoc/>
    public override string DisplayName => "Look At IK";

    /// <summary>
    /// The bone to rotate. Dropdown index where 0 = (none).
    /// </summary>
    [InspectorDropdown(nameof(GetBoneNames))]
    [InspectorLabel("Bone")]
    public int BoneIndex { get; set; }

    /// <summary>
    /// Coordinate space for <see cref="TargetPosition"/>.
    /// </summary>
    [InspectorSection("Target")]
    public IKTargetSpace TargetSpace { get; set; } = IKTargetSpace.World;

    /// <summary>
    /// Target position to look at.
    /// </summary>
    [InspectorGizmo("Target", ColorR = 50, ColorG = 255, ColorB = 50, SpaceProperty = nameof(TargetSpace))]
    public Vector3 TargetPosition { get; set; }

    /// <summary>
    /// Which local bone axis should point toward the target.
    /// </summary>
    [InspectorSection("Axes")]
    [InspectorLabel("Aim Axis")]
    public LocalAxis AimAxis { get; set; } = LocalAxis.ZPositive;

    /// <summary>
    /// Which local bone axis should align toward world up.
    /// </summary>
    [InspectorLabel("Up Axis")]
    public LocalAxis UpAxis { get; set; } = LocalAxis.YPositive;

    /// <inheritdoc/>
    public override bool IsConfigured => BoneIndex > 0;

    /// <inheritdoc/>
    public override bool CanSolve(BoneHierarchy hierarchy)
    {
        if (!IsConfigured)
            return false;
        int bone = BoneIndex - 1;
        return bone < hierarchy.BoneCount;
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

        int bone = BoneIndex - 1;

        var worldTarget = TargetSpace == IKTargetSpace.Local
            ? Vector3.Transform(TargetPosition, entityWorldMatrix)
            : TargetPosition;

        var bonePos = worldMatrices[bone].Translation;
        var toTarget = worldTarget - bonePos;
        if (toTarget.LengthSquared() < 1e-8f)
            return;

        var targetDir = Vector3.Normalize(toTarget);

        // Build look-at rotation: aim axis → target direction, up axis → world up
        var currentWorldRot = IKMath.ExtractRotation(worldMatrices[bone]);
        var currentAimDir = Vector3.Normalize(Vector3.Transform(GetAxisVector(AimAxis), Matrix4x4.CreateFromQuaternion(currentWorldRot)));

        // Rotation that aligns current aim direction to target direction
        var aimDelta = IKMath.RotationBetween(currentAimDir, targetDir);

        // Apply aim rotation
        var rotAfterAim = Quaternion.Normalize(Raymath.QuaternionMultiply(aimDelta, currentWorldRot));

        // Twist correction: align up axis toward world up
        var currentUpDir = Vector3.Normalize(Vector3.Transform(GetAxisVector(UpAxis), Matrix4x4.CreateFromQuaternion(rotAfterAim)));
        var worldUp = Vector3.UnitY;

        // Project world up onto the plane perpendicular to target direction
        var upProjected = worldUp - targetDir * Vector3.Dot(worldUp, targetDir);
        if (upProjected.LengthSquared() > 1e-6f)
        {
            upProjected = Vector3.Normalize(upProjected);
            // Project current up onto the same plane
            var currentUpProjected = currentUpDir - targetDir * Vector3.Dot(currentUpDir, targetDir);
            if (currentUpProjected.LengthSquared() > 1e-6f)
            {
                currentUpProjected = Vector3.Normalize(currentUpProjected);
                var twistDelta = IKMath.RotationBetween(currentUpProjected, upProjected);
                rotAfterAim = Quaternion.Normalize(Raymath.QuaternionMultiply(twistDelta, rotAfterAim));
            }
        }

        // Compute full world-space delta from current to desired
        var invCurrent = Raymath.QuaternionInvert(currentWorldRot);
        var fullDelta = Quaternion.Normalize(Raymath.QuaternionMultiply(rotAfterAim, invCurrent));

        if (Weight < 1f)
            fullDelta = Quaternion.Slerp(Quaternion.Identity, fullDelta, Weight);

        IKMath.ApplyWorldRotationDelta(localTransforms, worldMatrices, hierarchy, bone, fullDelta);
        IKMath.ForwardKinematics(hierarchy.ParentIndices, localTransforms, entityWorldMatrix, worldMatrices);
    }

    private static Vector3 GetAxisVector(LocalAxis axis) => axis switch
    {
        LocalAxis.XPositive => Vector3.UnitX,
        LocalAxis.YPositive => Vector3.UnitY,
        LocalAxis.ZPositive => Vector3.UnitZ,
        LocalAxis.XNegative => -Vector3.UnitX,
        LocalAxis.YNegative => -Vector3.UnitY,
        LocalAxis.ZNegative => -Vector3.UnitZ,
        _ => Vector3.UnitZ
    };
}
