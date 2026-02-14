using System.Buffers;
using System.Numerics;
using Raylib_cs;

namespace FrinkyEngine.Core.Animation.IK;

/// <summary>
/// Pure math utilities for inverse kinematics — no ECS or Raylib model dependencies.
/// </summary>
public static class IKMath
{
    /// <summary>
    /// Computes the desired mid-joint (elbow/knee) world position for a two-bone IK chain.
    /// Uses law of cosines to determine the triangle, and the pole target to orient the bend plane.
    /// Returns null if the chain is degenerate (zero-length bones or zero distance to target).
    /// </summary>
    public static Vector3? ComputeTwoBoneMidPosition(
        Vector3 rootPos, float upperLen, float lowerLen,
        Vector3 targetPos, Vector3 poleTarget)
    {
        if (upperLen < 1e-6f || lowerLen < 1e-6f)
            return null;

        var rootToTarget = targetPos - rootPos;
        float targetDist = rootToTarget.Length();

        if (targetDist < 1e-6f)
            return null;

        // Clamp to reachable range
        float clamped = Math.Clamp(targetDist, Math.Abs(upperLen - lowerLen) + 1e-4f, upperLen + lowerLen - 1e-4f);

        // Law of cosines: angle at root joint
        float cosAngle = (upperLen * upperLen + clamped * clamped - lowerLen * lowerLen) / (2f * upperLen * clamped);
        cosAngle = Math.Clamp(cosAngle, -1f, 1f);
        float angle = MathF.Acos(cosAngle);

        // Direction from root toward target (use actual target for direction, clamped distance for triangle)
        var targetDir = rootToTarget / targetDist;

        // Project pole target onto the plane perpendicular to targetDir, passing through root
        var poleDelta = poleTarget - rootPos;
        var poleOnAxis = targetDir * Vector3.Dot(poleDelta, targetDir);
        var polePerp = poleDelta - poleOnAxis;

        if (polePerp.LengthSquared() < 1e-6f)
        {
            // Pole is collinear with root→target — use arbitrary perpendicular
            polePerp = GetPerpendicular(targetDir);
        }
        polePerp = Vector3.Normalize(polePerp);

        // Mid position: rotate targetDir toward polePerp by the root angle, scale by upperLen
        var newMidPos = rootPos + (targetDir * MathF.Cos(angle) + polePerp * MathF.Sin(angle)) * upperLen;
        return newMidPos;
    }

    /// <summary>
    /// Computes world-space matrices for all bones via forward kinematics.
    /// </summary>
    public static void ForwardKinematics(
        int[] parentIndices,
        (Vector3 translation, Quaternion rotation, Vector3 scale)[] localTransforms,
        Matrix4x4 rootMatrix,
        Matrix4x4[] worldMatrices)
    {
        int count = Math.Min(parentIndices.Length, localTransforms.Length);
        count = Math.Min(count, worldMatrices.Length);
        if (count <= 0)
            return;

        // Handle arbitrary bone ordering; do not assume parent index < child index.
        var visitState = ArrayPool<byte>.Shared.Rent(count); // 0=unvisited, 1=visiting, 2=done
        Array.Clear(visitState, 0, count);
        for (int i = 0; i < count; i++)
            ComputeWorld(i);
        ArrayPool<byte>.Shared.Return(visitState);

        void ComputeWorld(int boneIndex)
        {
            if (visitState[boneIndex] == 2)
                return;
            bool isCycle = visitState[boneIndex] == 1;
            visitState[boneIndex] = 1;

            var (t, r, s) = localTransforms[boneIndex];
            var local = Matrix4x4.CreateScale(s)
                * Matrix4x4.CreateFromQuaternion(r)
                * Matrix4x4.CreateTranslation(t);

            int parent = parentIndices[boneIndex];
            if (!isCycle && parent >= 0 && parent < count)
            {
                ComputeWorld(parent);
                worldMatrices[boneIndex] = local * worldMatrices[parent];
            }
            else
            {
                worldMatrices[boneIndex] = local * rootMatrix;
            }

            visitState[boneIndex] = 2;
        }
    }

    /// <summary>
    /// Extracts the rotation as a quaternion from a 4x4 matrix.
    /// Assumes the matrix is a valid TRS matrix.
    /// </summary>
    public static Quaternion ExtractRotation(Matrix4x4 m)
    {
        Matrix4x4.Decompose(m, out _, out var rotation, out _);
        return rotation;
    }

    /// <summary>
    /// Converts a world-space rotation to local space given the parent's world matrix.
    /// </summary>
    public static Quaternion WorldToLocalRotation(Quaternion worldRot, Matrix4x4 parentWorldMatrix)
    {
        var parentRot = ExtractRotation(parentWorldMatrix);
        var invParent = Raymath.QuaternionInvert(parentRot);
        return Quaternion.Normalize(Raymath.QuaternionMultiply(invParent, worldRot));
    }

    /// <summary>
    /// Computes the shortest rotation quaternion that rotates vector <paramref name="from"/> to <paramref name="to"/>.
    /// </summary>
    public static Quaternion RotationBetween(Vector3 from, Vector3 to)
    {
        from = Vector3.Normalize(from);
        to = Vector3.Normalize(to);

        float dot = Vector3.Dot(from, to);

        if (dot > 0.999999f)
            return Quaternion.Identity;

        if (dot < -0.999999f)
        {
            // 180 degree rotation — pick an arbitrary perpendicular axis
            var axis = GetPerpendicular(from);
            return Quaternion.CreateFromAxisAngle(axis, MathF.PI);
        }

        var cross = Vector3.Cross(from, to);
        return Quaternion.Normalize(new Quaternion(cross, 1f + dot));
    }

    /// <summary>
    /// Applies a world-space rotation delta to a bone, converting back to local space.
    /// Updates the local transform in place.
    /// </summary>
    public static void ApplyWorldRotationDelta(
        (Vector3 translation, Quaternion rotation, Vector3 scale)[] localTransforms,
        Matrix4x4[] worldMatrices,
        BoneHierarchy hierarchy,
        int boneIndex,
        Quaternion worldDelta)
    {
        var currentWorldRot = ExtractRotation(worldMatrices[boneIndex]);
        var newWorldRot = Quaternion.Normalize(Raymath.QuaternionMultiply(worldDelta, currentWorldRot));

        int parentIdx = hierarchy.ParentIndices[boneIndex];
        Quaternion newLocalRot;
        if (parentIdx >= 0 && parentIdx < worldMatrices.Length)
            newLocalRot = WorldToLocalRotation(newWorldRot, worldMatrices[parentIdx]);
        else
            newLocalRot = newWorldRot;

        var (t, _, s) = localTransforms[boneIndex];
        localTransforms[boneIndex] = (t, Quaternion.Normalize(newLocalRot), s);
    }

    private static Vector3 GetPerpendicular(Vector3 v)
    {
        v = Vector3.Normalize(v);
        var candidate = Math.Abs(v.X) < 0.9f ? Vector3.UnitX : Vector3.UnitY;
        return Vector3.Normalize(Vector3.Cross(v, candidate));
    }
}
