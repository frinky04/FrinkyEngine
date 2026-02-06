using System.Numerics;
using FrinkyEngine.Core.ECS;

namespace FrinkyEngine.Core.Components;

public class TransformComponent : Component
{
    private Vector3 _localPosition = Vector3.Zero;
    private Quaternion _localRotation = Quaternion.Identity;
    private Vector3 _localScale = Vector3.One;
    private TransformComponent? _parent;
    private readonly List<TransformComponent> _children = new();
    private Vector3 _cachedEuler = Vector3.Zero;
    private bool _eulerDirty = true;

    public Vector3 LocalPosition
    {
        get => _localPosition;
        set => _localPosition = value;
    }

    public Quaternion LocalRotation
    {
        get => _localRotation;
        set
        {
            _localRotation = value;
            _eulerDirty = true;
        }
    }

    public Vector3 LocalScale
    {
        get => _localScale;
        set => _localScale = value;
    }

    public Vector3 EulerAngles
    {
        get
        {
            if (_eulerDirty)
            {
                _cachedEuler = QuaternionToEuler(_localRotation);
                _eulerDirty = false;
            }
            return _cachedEuler;
        }
        set
        {
            _cachedEuler = value;
            _eulerDirty = false;
            _localRotation = EulerToQuaternion(value);
        }
    }

    public TransformComponent? Parent => _parent;
    public IReadOnlyList<TransformComponent> Children => _children;

    public Vector3 WorldPosition
    {
        get
        {
            var world = WorldMatrix;
            return new Vector3(world.M41, world.M42, world.M43);
        }
    }

    public Matrix4x4 LocalMatrix
    {
        get
        {
            return Matrix4x4.CreateScale(_localScale) *
                   Matrix4x4.CreateFromQuaternion(_localRotation) *
                   Matrix4x4.CreateTranslation(_localPosition);
        }
    }

    public Matrix4x4 WorldMatrix
    {
        get
        {
            if (_parent != null)
                return LocalMatrix * _parent.WorldMatrix;
            return LocalMatrix;
        }
    }

    public Vector3 Forward
    {
        get
        {
            var rot = _parent != null
                ? _localRotation * _parent.WorldRotation
                : _localRotation;
            return Vector3.Transform(-Vector3.UnitZ, rot);
        }
    }

    public Vector3 Right
    {
        get
        {
            var rot = _parent != null
                ? _localRotation * _parent.WorldRotation
                : _localRotation;
            return Vector3.Transform(Vector3.UnitX, rot);
        }
    }

    public Vector3 Up
    {
        get
        {
            var rot = _parent != null
                ? _localRotation * _parent.WorldRotation
                : _localRotation;
            return Vector3.Transform(Vector3.UnitY, rot);
        }
    }

    public Quaternion WorldRotation
    {
        get
        {
            if (_parent != null)
                return _localRotation * _parent.WorldRotation;
            return _localRotation;
        }
    }

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
