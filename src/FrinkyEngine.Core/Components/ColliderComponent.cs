using System.Numerics;
using FrinkyEngine.Core.ECS;

namespace FrinkyEngine.Core.Components;

/// <summary>
/// Base component for all physics collider shapes.
/// </summary>
public abstract class ColliderComponent : Component
{
    private float _friction = 0.8f;
    private float _restitution = 0.0f;
    private Vector3 _center = Vector3.Zero;
    private int _settingsVersion;

    /// <summary>
    /// Surface friction coefficient used during collision response.
    /// </summary>
    public float Friction
    {
        get => _friction;
        set
        {
            var clamped = MathF.Max(0f, value);
            if (_friction == clamped)
                return;

            _friction = clamped;
            MarkColliderDirty();
        }
    }

    /// <summary>
    /// Bounciness value in range [0, 1].
    /// </summary>
    public float Restitution
    {
        get => _restitution;
        set
        {
            var clamped = Math.Clamp(value, 0f, 1f);
            if (_restitution == clamped)
                return;

            _restitution = clamped;
            MarkColliderDirty();
        }
    }

    /// <summary>
    /// Local offset applied to the collider relative to the entity transform.
    /// </summary>
    public Vector3 Center
    {
        get => _center;
        set
        {
            if (_center == value)
                return;

            _center = value;
            MarkColliderDirty();
        }
    }

    internal int SettingsVersion => _settingsVersion;

    /// <summary>
    /// Marks collider data as changed so physics can rebuild representation if needed.
    /// </summary>
    protected void MarkColliderDirty()
    {
        unchecked
        {
            _settingsVersion++;
        }
        NotifyPhysicsChanged();
    }

    /// <inheritdoc />
    public override void OnEnable() => NotifyPhysicsChanged();

    /// <inheritdoc />
    public override void OnDisable() => NotifyPhysicsChanged();

    /// <inheritdoc />
    public override void OnDestroy() => NotifyPhysicsChanged();

    private void NotifyPhysicsChanged()
    {
        Entity.Scene?.NotifyPhysicsStateChanged();
    }
}
