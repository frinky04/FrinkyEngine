using System.Numerics;
using FrinkyEngine.Core.ECS;

namespace FrinkyEngine.Core.Components;

/// <summary>
/// Physics body component used by the scene physics system.
/// </summary>
[ComponentCategory("Physics")]
public class RigidbodyComponent : Component
{
    private BodyMotionType _motionType = BodyMotionType.Dynamic;
    private float _mass = 1.0f;
    private float _linearDamping = 0.03f;
    private float _angularDamping = 0.03f;
    private bool _continuousDetection;
    private bool _lockPositionX;
    private bool _lockPositionY;
    private bool _lockPositionZ;
    private bool _lockRotationX;
    private bool _lockRotationY;
    private bool _lockRotationZ;
    private int _settingsVersion;
    private Vector3 _pendingForce;
    private Vector3 _pendingImpulse;
    private Vector3 _initialLinearVelocity;

    /// <summary>
    /// How the body should be simulated.
    /// </summary>
    public BodyMotionType MotionType
    {
        get => _motionType;
        set
        {
            if (_motionType == value)
                return;

            _motionType = value;
            MarkBodyDirty();
        }
    }

    /// <summary>
    /// Body mass used when <see cref="MotionType"/> is <see cref="BodyMotionType.Dynamic"/>.
    /// </summary>
    public float Mass
    {
        get => _mass;
        set
        {
            var clamped = MathF.Max(0.0001f, value);
            if (_mass == clamped)
                return;

            _mass = clamped;
            MarkBodyDirty();
        }
    }

    /// <summary>
    /// Fraction of linear velocity removed per second in range [0, 1].
    /// </summary>
    public float LinearDamping
    {
        get => _linearDamping;
        set
        {
            var clamped = Math.Clamp(value, 0f, 1f);
            if (_linearDamping == clamped)
                return;

            _linearDamping = clamped;
            MarkBodyDirty();
        }
    }

    /// <summary>
    /// Fraction of angular velocity removed per second in range [0, 1].
    /// </summary>
    public float AngularDamping
    {
        get => _angularDamping;
        set
        {
            var clamped = Math.Clamp(value, 0f, 1f);
            if (_angularDamping == clamped)
                return;

            _angularDamping = clamped;
            MarkBodyDirty();
        }
    }

    /// <summary>
    /// Enables continuous collision detection for the body collidable.
    /// </summary>
    public bool ContinuousDetection
    {
        get => _continuousDetection;
        set
        {
            if (_continuousDetection == value)
                return;

            _continuousDetection = value;
            MarkBodyDirty();
        }
    }

    /// <summary>
    /// Locks translation on world X.
    /// </summary>
    public bool LockPositionX
    {
        get => _lockPositionX;
        set
        {
            if (_lockPositionX == value)
                return;
            _lockPositionX = value;
            MarkBodyDirty();
        }
    }

    /// <summary>
    /// Locks translation on world Y.
    /// </summary>
    public bool LockPositionY
    {
        get => _lockPositionY;
        set
        {
            if (_lockPositionY == value)
                return;
            _lockPositionY = value;
            MarkBodyDirty();
        }
    }

    /// <summary>
    /// Locks translation on world Z.
    /// </summary>
    public bool LockPositionZ
    {
        get => _lockPositionZ;
        set
        {
            if (_lockPositionZ == value)
                return;
            _lockPositionZ = value;
            MarkBodyDirty();
        }
    }

    /// <summary>
    /// Locks rotation around world X.
    /// </summary>
    public bool LockRotationX
    {
        get => _lockRotationX;
        set
        {
            if (_lockRotationX == value)
                return;
            _lockRotationX = value;
            MarkBodyDirty();
        }
    }

    /// <summary>
    /// Locks rotation around world Y.
    /// </summary>
    public bool LockRotationY
    {
        get => _lockRotationY;
        set
        {
            if (_lockRotationY == value)
                return;
            _lockRotationY = value;
            MarkBodyDirty();
        }
    }

    /// <summary>
    /// Locks rotation around world Z.
    /// </summary>
    public bool LockRotationZ
    {
        get => _lockRotationZ;
        set
        {
            if (_lockRotationZ == value)
                return;
            _lockRotationZ = value;
            MarkBodyDirty();
        }
    }

    internal int SettingsVersion => _settingsVersion;

    internal Vector3 InitialLinearVelocity
    {
        get => _initialLinearVelocity;
        set => _initialLinearVelocity = value;
    }

    /// <summary>
    /// Adds a force that will be applied during the next physics step.
    /// </summary>
    public void ApplyForce(Vector3 force)
    {
        _pendingForce += force;
    }

    /// <summary>
    /// Adds an instantaneous linear impulse.
    /// </summary>
    public void ApplyImpulse(Vector3 impulse)
    {
        _pendingImpulse += impulse;
    }

    /// <summary>
    /// Sets the rigidbody linear velocity.
    /// </summary>
    public void SetLinearVelocity(Vector3 velocity)
    {
        _initialLinearVelocity = velocity;
        var physicsSystem = Entity.Scene?.PhysicsSystem;
        physicsSystem?.SetLinearVelocity(this, velocity);
    }

    /// <summary>
    /// Gets the current linear velocity.
    /// </summary>
    public Vector3 GetLinearVelocity()
    {
        var physicsSystem = Entity.Scene?.PhysicsSystem;
        if (physicsSystem != null && physicsSystem.TryGetLinearVelocity(this, out var velocity))
            return velocity;

        return _initialLinearVelocity;
    }

    internal void ConsumePendingForces(out Vector3 force, out Vector3 impulse)
    {
        force = _pendingForce;
        impulse = _pendingImpulse;
        _pendingForce = Vector3.Zero;
        _pendingImpulse = Vector3.Zero;
    }

    /// <inheritdoc />
    public override void OnEnable() => NotifyPhysicsChanged();

    /// <inheritdoc />
    public override void OnDisable() => NotifyPhysicsChanged();

    /// <inheritdoc />
    public override void OnDestroy() => NotifyPhysicsChanged();

    private void MarkBodyDirty()
    {
        unchecked
        {
            _settingsVersion++;
        }
        NotifyPhysicsChanged();
    }

    private void NotifyPhysicsChanged()
    {
        Entity.Scene?.NotifyPhysicsStateChanged();
    }
}
