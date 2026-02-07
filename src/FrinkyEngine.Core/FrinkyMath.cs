using System.Numerics;

namespace FrinkyEngine.Core;

/// <summary>
/// Common math constants and conversion utilities used throughout the engine.
/// </summary>
public static class FrinkyMath
{
    /// <summary>
    /// Multiplier to convert degrees to radians.
    /// </summary>
    public const float Deg2Rad = MathF.PI / 180f;

    /// <summary>
    /// Multiplier to convert radians to degrees.
    /// </summary>
    public const float Rad2Deg = 180f / MathF.PI;

    /// <summary>
    /// Converts a quaternion rotation to Euler angles in degrees.
    /// </summary>
    /// <param name="q">The quaternion to convert.</param>
    /// <returns>A <see cref="Vector3"/> containing (roll, pitch, yaw) in degrees.</returns>
    public static Vector3 QuaternionToEuler(Quaternion q)
    {
        float sinr_cosp = 2f * (q.W * q.X + q.Y * q.Z);
        float cosr_cosp = 1f - 2f * (q.X * q.X + q.Y * q.Y);
        float roll = MathF.Atan2(sinr_cosp, cosr_cosp);

        float sinp = 2f * (q.W * q.Y - q.Z * q.X);
        float pitch = MathF.Abs(sinp) >= 1f
            ? MathF.CopySign(MathF.PI / 2f, sinp)
            : MathF.Asin(sinp);

        float siny_cosp = 2f * (q.W * q.Z + q.X * q.Y);
        float cosy_cosp = 1f - 2f * (q.Y * q.Y + q.Z * q.Z);
        float yaw = MathF.Atan2(siny_cosp, cosy_cosp);

        return new Vector3(roll * Rad2Deg, pitch * Rad2Deg, yaw * Rad2Deg);
    }

    /// <summary>
    /// Converts Euler angles in degrees to a quaternion rotation.
    /// </summary>
    /// <param name="eulerDegrees">A <see cref="Vector3"/> containing (pitch, yaw, roll) in degrees.</param>
    /// <returns>The equivalent quaternion rotation.</returns>
    public static Quaternion EulerToQuaternion(Vector3 eulerDegrees)
    {
        return Quaternion.CreateFromYawPitchRoll(
            eulerDegrees.Y * Deg2Rad,
            eulerDegrees.X * Deg2Rad,
            eulerDegrees.Z * Deg2Rad);
    }

    /// <summary>
    /// Converts a <see cref="Matrix4x4"/> to a flat 16-element float array in row-major order.
    /// </summary>
    /// <param name="m">The matrix to convert.</param>
    /// <returns>A 16-element array containing the matrix values.</returns>
    public static float[] Matrix4x4ToFloatArray(Matrix4x4 m)
    {
        return new[]
        {
            m.M11, m.M12, m.M13, m.M14,
            m.M21, m.M22, m.M23, m.M24,
            m.M31, m.M32, m.M33, m.M34,
            m.M41, m.M42, m.M43, m.M44
        };
    }
}
