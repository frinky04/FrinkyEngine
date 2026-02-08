using System.Numerics;
using FrinkyEngine.Core.ECS;

namespace FrinkyEngine.Core.Components;

/// <summary>
/// Defines an entity's position, rotation, and scale in 3D space.
/// Supports parent-child hierarchies for nested transforms.
/// </summary>
/// <remarks>
/// Every <see cref="Entity"/> always has exactly one <see cref="TransformComponent"/> that cannot be removed.
/// Local properties are relative to the parent transform; world properties account for the full hierarchy.
/// </remarks>
public class TransformComponent : Component
{
    private Vector3 _authoritativeLocalPosition = Vector3.Zero;
    private Quaternion _authoritativeLocalRotation = Quaternion.Identity;
    private Vector3 _visualLocalPosition = Vector3.Zero;
    private Quaternion _visualLocalRotation = Quaternion.Identity;
    private bool _hasVisualPoseOverride;
    private Vector3 _localScale = Vector3.One;
    private TransformComponent? _parent;
    private readonly List<TransformComponent> _children = new();
    private Vector3 _cachedEuler = Vector3.Zero;
    private bool _eulerDirty = true;

    /// <summary>
    /// Position relative to the parent transform (or world origin if no parent).
    /// </summary>
    public Vector3 LocalPosition
    {
        get => _hasVisualPoseOverride ? _visualLocalPosition : _authoritativeLocalPosition;
        set
        {
            _authoritativeLocalPosition = value;
            if (_hasVisualPoseOverride)
                _visualLocalPosition = value;
        }
    }

    /// <summary>
    /// Rotation relative to the parent transform as a quaternion.
    /// </summary>
    public Quaternion LocalRotation
    {
        get => _hasVisualPoseOverride ? _visualLocalRotation : _authoritativeLocalRotation;
        set
        {
            _authoritativeLocalRotation = value;
            if (_hasVisualPoseOverride)
                _visualLocalRotation = value;
            _eulerDirty = true;
        }
    }

    /// <summary>
    /// Scale relative to the parent transform (defaults to <c>(1, 1, 1)</c>).
    /// </summary>
    public Vector3 LocalScale
    {
        get => _localScale;
        set => _localScale = value;
    }

    /// <summary>
    /// Local rotation expressed as Euler angles in degrees (X = pitch, Y = yaw, Z = roll).
    /// </summary>
    public Vector3 EulerAngles
    {
        get
        {
            if (_eulerDirty)
            {
                _cachedEuler = QuaternionToEuler(LocalRotation);
                _eulerDirty = false;
            }
            return _cachedEuler;
        }
        set
        {
            _cachedEuler = value;
            _eulerDirty = false;
            var rotation = EulerToQuaternion(value);
            _authoritativeLocalRotation = rotation;
            if (_hasVisualPoseOverride)
                _visualLocalRotation = rotation;
        }
    }

    /// <summary>
    /// The parent transform in the hierarchy, or <c>null</c> if this is a root transform.
    /// </summary>
    public TransformComponent? Parent => _parent;

    /// <summary>
    /// The immediate child transforms in the hierarchy.
    /// </summary>
    public IReadOnlyList<TransformComponent> Children => _children;

    /// <summary>
    /// The position in world space, computed from the full hierarchy.
    /// </summary>
    public Vector3 WorldPosition
    {
        get
        {
            var world = GetWorldMatrix(useVisualPose: true);
            return new Vector3(world.M41, world.M42, world.M43);
        }
        set
        {
            if (_parent != null)
            {
                if (Matrix4x4.Invert(_parent.GetWorldMatrix(useVisualPose: false), out var parentInverse))
                {
                    _authoritativeLocalPosition = Vector3.Transform(value, parentInverse);
                }
                else
                {
                    _authoritativeLocalPosition = value;
                }
            }
            else
            {
                _authoritativeLocalPosition = value;
            }

            if (_hasVisualPoseOverride)
                _visualLocalPosition = _authoritativeLocalPosition;
        }
    }

    /// <summary>
    /// The local transform matrix (scale * rotation * translation).
    /// </summary>
    public Matrix4x4 LocalMatrix
    {
        get => GetLocalMatrix(useVisualPose: true);
    }

    /// <summary>
    /// The world transform matrix, combining this transform with all parent transforms.
    /// </summary>
    public Matrix4x4 WorldMatrix
    {
        get => GetWorldMatrix(useVisualPose: true);
    }

    /// <summary>
    /// The forward direction (negative Z axis) in world space.
    /// </summary>
    public Vector3 Forward
    {
        get
        {
            var rot = GetWorldRotation(useVisualPose: true);
            return Vector3.Transform(-Vector3.UnitZ, rot);
        }
    }

    /// <summary>
    /// The right direction (positive X axis) in world space.
    /// </summary>
    public Vector3 Right
    {
        get
        {
            var rot = GetWorldRotation(useVisualPose: true);
            return Vector3.Transform(Vector3.UnitX, rot);
        }
    }

    /// <summary>
    /// The up direction (positive Y axis) in world space.
    /// </summary>
    public Vector3 Up
    {
        get
        {
            var rot = GetWorldRotation(useVisualPose: true);
            return Vector3.Transform(Vector3.UnitY, rot);
        }
    }

    /// <summary>
    /// The rotation in world space, combining local rotation with all parent rotations.
    /// </summary>
    public Quaternion WorldRotation
    {
        get => GetWorldRotation(useVisualPose: true);
        set
        {
            if (_parent != null)
            {
                var parentInverse = Quaternion.Inverse(_parent.GetWorldRotation(useVisualPose: false));
                _authoritativeLocalRotation = NormalizeOrIdentity(value * parentInverse);
            }
            else
            {
                _authoritativeLocalRotation = NormalizeOrIdentity(value);
            }

            if (_hasVisualPoseOverride)
                _visualLocalRotation = _authoritativeLocalRotation;
            _eulerDirty = true;
        }
    }

    internal bool HasVisualPoseOverride => _hasVisualPoseOverride;

    internal (Vector3 Position, Quaternion Rotation) GetAuthoritativeLocalPose()
    {
        return (_authoritativeLocalPosition, _authoritativeLocalRotation);
    }

    internal void SetAuthoritativeLocalPose(Vector3 position, Quaternion rotation)
    {
        _authoritativeLocalPosition = position;
        _authoritativeLocalRotation = rotation;
        if (_hasVisualPoseOverride)
        {
            _visualLocalPosition = position;
            _visualLocalRotation = rotation;
        }
        _eulerDirty = true;
    }

    internal void SetVisualLocalPoseOverride(Vector3 position, Quaternion rotation)
    {
        _visualLocalPosition = position;
        _visualLocalRotation = rotation;
        _hasVisualPoseOverride = true;
        _eulerDirty = true;
    }

    internal void ClearVisualPoseOverride()
    {
        if (!_hasVisualPoseOverride)
            return;

        _hasVisualPoseOverride = false;
        _eulerDirty = true;
    }

    internal Matrix4x4 GetAuthoritativeWorldMatrix()
    {
        return GetWorldMatrix(useVisualPose: false);
    }

    internal Quaternion GetAuthoritativeWorldRotation()
    {
        return GetWorldRotation(useVisualPose: false);
    }

    /// <summary>
    /// Sets a new parent for this transform, updating the hierarchy.
    /// </summary>
    /// <param name="newParent">The new parent transform, or <c>null</c> to make this a root transform.</param>
    /// <remarks>
    /// Circular hierarchies are prevented — if this transform is an ancestor of <paramref name="newParent"/>, the call is ignored.
    /// </remarks>
    public void SetParent(TransformComponent? newParent)
    {
        if (newParent == this) return;
        if (newParent != null && IsAncestorOf(newParent)) return;

        _parent?.RemoveChildInternal(this);
        _parent = newParent;
        _parent?.AddChildInternal(this);
    }

    private bool IsAncestorOf(TransformComponent other)
    {
        var current = other._parent;
        while (current != null)
        {
            if (current == this) return true;
            current = current._parent;
        }
        return false;
    }

    private void AddChildInternal(TransformComponent child) => _children.Add(child);
    private void RemoveChildInternal(TransformComponent child) => _children.Remove(child);

    /// <summary>
    /// Transforms a point from local space to world space.
    /// </summary>
    public Vector3 TransformPoint(Vector3 point) => Vector3.Transform(point, WorldMatrix);

    /// <summary>
    /// Transforms a point from world space to local space.
    /// </summary>
    public Vector3 InverseTransformPoint(Vector3 point)
    {
        if (Matrix4x4.Invert(WorldMatrix, out var inverse))
            return Vector3.Transform(point, inverse);
        return point;
    }

    /// <summary>
    /// Transforms a direction from local space to world space (rotation only, ignores scale).
    /// </summary>
    public Vector3 TransformDirection(Vector3 direction) => Vector3.Transform(direction, WorldRotation);

    /// <summary>
    /// Transforms a direction from world space to local space (rotation only, ignores scale).
    /// </summary>
    public Vector3 InverseTransformDirection(Vector3 direction) => Vector3.Transform(direction, Quaternion.Inverse(WorldRotation));

    /// <summary>
    /// Transforms a vector from local space to world space (rotation and scale).
    /// </summary>
    public Vector3 TransformVector(Vector3 vector) => Vector3.TransformNormal(vector, WorldMatrix);

    /// <summary>
    /// Transforms a vector from world space to local space (rotation and scale).
    /// </summary>
    public Vector3 InverseTransformVector(Vector3 vector)
    {
        if (Matrix4x4.Invert(WorldMatrix, out var inverse))
            return Vector3.TransformNormal(vector, inverse);
        return vector;
    }

    private Matrix4x4 GetLocalMatrix(bool useVisualPose)
    {
        var position = useVisualPose && _hasVisualPoseOverride ? _visualLocalPosition : _authoritativeLocalPosition;
        var rotation = useVisualPose && _hasVisualPoseOverride ? _visualLocalRotation : _authoritativeLocalRotation;
        return Matrix4x4.CreateScale(_localScale) *
               Matrix4x4.CreateFromQuaternion(rotation) *
               Matrix4x4.CreateTranslation(position);
    }

    private Matrix4x4 GetWorldMatrix(bool useVisualPose)
    {
        if (_parent != null)
            return GetLocalMatrix(useVisualPose) * _parent.GetWorldMatrix(useVisualPose);
        return GetLocalMatrix(useVisualPose);
    }

    private Quaternion GetWorldRotation(bool useVisualPose)
    {
        var local = useVisualPose && _hasVisualPoseOverride ? _visualLocalRotation : _authoritativeLocalRotation;
        if (_parent != null)
            return local * _parent.GetWorldRotation(useVisualPose);
        return local;
    }

    private static Quaternion NormalizeOrIdentity(Quaternion rotation)
    {
        var lengthSquared = rotation.LengthSquared();
        if (!float.IsFinite(lengthSquared) || lengthSquared <= 1e-12f)
            return Quaternion.Identity;
        return Quaternion.Normalize(rotation);
    }

    private static Vector3 QuaternionToEuler(Quaternion q)
    {
        const float rad2deg = 180f / MathF.PI;

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

        return new Vector3(roll * rad2deg, pitch * rad2deg, yaw * rad2deg);
    }

    private static Quaternion EulerToQuaternion(Vector3 eulerDegrees)
    {
        const float deg2rad = MathF.PI / 180f;
        return Quaternion.CreateFromYawPitchRoll(
            eulerDegrees.Y * deg2rad,
            eulerDegrees.X * deg2rad,
            eulerDegrees.Z * deg2rad);
    }
}
