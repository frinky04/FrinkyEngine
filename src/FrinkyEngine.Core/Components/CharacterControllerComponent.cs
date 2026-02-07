using System.Numerics;
using FrinkyEngine.Core.ECS;

namespace FrinkyEngine.Core.Components;

/// <summary>
/// High-level dynamic character locomotion component backed by BEPU's character support constraints.
/// Requires an enabled <see cref="RigidbodyComponent"/> and <see cref="CapsuleColliderComponent"/> on the same entity.
/// </summary>
public class CharacterControllerComponent : Component
{
    private float _moveSpeed = 4f;
    private float _jumpVelocity = 6f;
    private float _maxSlopeDegrees = 45f;
    private float _maximumHorizontalForce = 20f;
    private float _maximumVerticalForce = 100f;
    private float _airControlForceScale = 0.2f;
    private float _airControlSpeedScale = 0.2f;
    private bool _useEntityForwardAsViewDirection = true;
    private Vector3 _viewDirectionOverride = -Vector3.UnitZ;
    private int _settingsVersion;

    private Vector3 _pendingWorldMovementInput;
    private Vector2 _pendingPlanarInput;
    private bool _hasPendingPlanarInput;
    private Vector3 _pendingSlideVelocity;
    private bool _hasPendingSlideVelocity;
    private bool _jumpRequested;

    private bool _supported;
    private Vector3 _lastComputedTargetVelocity;

    /// <summary>
    /// Maximum supported horizontal speed for input-driven movement.
    /// </summary>
    public float MoveSpeed
    {
        get => _moveSpeed;
        set
        {
            var clamped = float.IsFinite(value) ? MathF.Max(0f, value) : 4f;
            if (_moveSpeed == clamped)
                return;

            _moveSpeed = clamped;
            MarkControllerDirty();
        }
    }

    /// <summary>
    /// Upward launch speed used when a jump is requested while supported.
    /// </summary>
    public float JumpVelocity
    {
        get => _jumpVelocity;
        set
        {
            var clamped = float.IsFinite(value) ? MathF.Max(0f, value) : 6f;
            if (_jumpVelocity == clamped)
                return;

            _jumpVelocity = clamped;
            MarkControllerDirty();
        }
    }

    /// <summary>
    /// Maximum walkable slope angle in degrees.
    /// </summary>
    public float MaxSlopeDegrees
    {
        get => _maxSlopeDegrees;
        set
        {
            var clamped = float.IsFinite(value) ? Math.Clamp(value, 0f, 89f) : 45f;
            if (_maxSlopeDegrees == clamped)
                return;

            _maxSlopeDegrees = clamped;
            MarkControllerDirty();
        }
    }

    /// <summary>
    /// Maximum horizontal force applied by support constraints.
    /// </summary>
    public float MaximumHorizontalForce
    {
        get => _maximumHorizontalForce;
        set
        {
            var clamped = float.IsFinite(value) ? MathF.Max(0f, value) : 20f;
            if (_maximumHorizontalForce == clamped)
                return;

            _maximumHorizontalForce = clamped;
            MarkControllerDirty();
        }
    }

    /// <summary>
    /// Maximum vertical force used to maintain support contact.
    /// </summary>
    public float MaximumVerticalForce
    {
        get => _maximumVerticalForce;
        set
        {
            var clamped = float.IsFinite(value) ? MathF.Max(0f, value) : 100f;
            if (_maximumVerticalForce == clamped)
                return;

            _maximumVerticalForce = clamped;
            MarkControllerDirty();
        }
    }

    /// <summary>
    /// Air control acceleration force scale applied while unsupported.
    /// </summary>
    public float AirControlForceScale
    {
        get => _airControlForceScale;
        set
        {
            var clamped = float.IsFinite(value) ? MathF.Max(0f, value) : 0.2f;
            if (_airControlForceScale == clamped)
                return;

            _airControlForceScale = clamped;
            MarkControllerDirty();
        }
    }

    /// <summary>
    /// Air control speed cap scale relative to desired movement speed.
    /// </summary>
    public float AirControlSpeedScale
    {
        get => _airControlSpeedScale;
        set
        {
            var clamped = float.IsFinite(value) ? MathF.Max(0f, value) : 0.2f;
            if (_airControlSpeedScale == clamped)
                return;

            _airControlSpeedScale = clamped;
            MarkControllerDirty();
        }
    }

    /// <summary>
    /// When true, view direction is read from <see cref="TransformComponent.Forward"/>.
    /// </summary>
    public bool UseEntityForwardAsViewDirection
    {
        get => _useEntityForwardAsViewDirection;
        set
        {
            if (_useEntityForwardAsViewDirection == value)
                return;

            _useEntityForwardAsViewDirection = value;
            MarkControllerDirty();
        }
    }

    /// <summary>
    /// View direction used when <see cref="UseEntityForwardAsViewDirection"/> is false.
    /// </summary>
    public Vector3 ViewDirectionOverride
    {
        get => _viewDirectionOverride;
        set
        {
            if (_viewDirectionOverride == value)
                return;

            _viewDirectionOverride = value;
            MarkControllerDirty();
        }
    }

    /// <summary>
    /// True when the character is currently supported by a walkable contact.
    /// </summary>
    public bool Supported => _supported;

    /// <summary>
    /// Last target horizontal world velocity computed from input this frame.
    /// </summary>
    public Vector3 LastComputedTargetVelocity => _lastComputedTargetVelocity;

    internal int SettingsVersion => _settingsVersion;

    /// <summary>
    /// Unreal-style movement input accumulator.
    /// </summary>
    public void AddMovementInput(Vector3 worldDirection, float scale = 1f)
    {
        if (!float.IsFinite(scale) || scale == 0f)
            return;
        if (!IsFinite(worldDirection))
            return;

        var lengthSquared = worldDirection.LengthSquared();
        if (lengthSquared <= 1e-12f)
            return;

        _pendingWorldMovementInput += worldDirection / MathF.Sqrt(lengthSquared) * scale;
    }

    /// <summary>
    /// Sets direct planar movement input (X = strafe, Y = forward).
    /// </summary>
    public void SetMoveInput(Vector2 input)
    {
        _pendingPlanarInput = new Vector2(
            float.IsFinite(input.X) ? input.X : 0f,
            float.IsFinite(input.Y) ? input.Y : 0f);
        _hasPendingPlanarInput = true;
    }

    /// <summary>
    /// Requests a jump attempt on the next physics step.
    /// </summary>
    public void Jump()
    {
        _jumpRequested = true;
    }

    /// <summary>
    /// Godot-style convenience call for setting desired movement velocity and optional jump request.
    /// </summary>
    public void MoveAndSlide(Vector3 desiredWorldVelocity, bool requestJump = false)
    {
        _pendingSlideVelocity = IsFinite(desiredWorldVelocity)
            ? desiredWorldVelocity
            : Vector3.Zero;
        _hasPendingSlideVelocity = true;
        if (requestJump)
            _jumpRequested = true;
    }

    /// <summary>
    /// Returns true when the character has support contact.
    /// </summary>
    public bool IsOnFloor() => _supported;

    /// <summary>
    /// Returns the current linear velocity of the attached rigidbody, if available.
    /// </summary>
    public Vector3 GetVelocity()
    {
        var rigidbody = Entity.GetComponent<RigidbodyComponent>();
        return rigidbody?.GetLinearVelocity() ?? Vector3.Zero;
    }

    internal CharacterControllerInputSnapshot CaptureInputSnapshot()
    {
        return new CharacterControllerInputSnapshot(
            _pendingWorldMovementInput,
            _pendingPlanarInput,
            _hasPendingPlanarInput,
            _pendingSlideVelocity,
            _hasPendingSlideVelocity,
            _jumpRequested);
    }

    internal void ConsumeInputSnapshot()
    {
        _pendingWorldMovementInput = Vector3.Zero;
        _pendingPlanarInput = Vector2.Zero;
        _hasPendingPlanarInput = false;
        _pendingSlideVelocity = Vector3.Zero;
        _hasPendingSlideVelocity = false;
        _jumpRequested = false;
    }

    internal void SetRuntimeState(bool supported, Vector3 lastComputedTargetVelocity)
    {
        _supported = supported;
        _lastComputedTargetVelocity = lastComputedTargetVelocity;
    }

    /// <inheritdoc />
    public override void OnEnable() => NotifyPhysicsChanged();

    /// <inheritdoc />
    public override void OnDisable() => NotifyPhysicsChanged();

    /// <inheritdoc />
    public override void OnDestroy() => NotifyPhysicsChanged();

    private void MarkControllerDirty()
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

    private static bool IsFinite(Vector3 value)
    {
        return float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Z);
    }
}

internal readonly struct CharacterControllerInputSnapshot
{
    public CharacterControllerInputSnapshot(
        Vector3 worldMovementInput,
        Vector2 planarInput,
        bool hasPlanarInput,
        Vector3 slideVelocity,
        bool hasSlideVelocity,
        bool jumpRequested)
    {
        WorldMovementInput = worldMovementInput;
        PlanarInput = planarInput;
        HasPlanarInput = hasPlanarInput;
        SlideVelocity = slideVelocity;
        HasSlideVelocity = hasSlideVelocity;
        JumpRequested = jumpRequested;
    }

    public Vector3 WorldMovementInput { get; }
    public Vector2 PlanarInput { get; }
    public bool HasPlanarInput { get; }
    public Vector3 SlideVelocity { get; }
    public bool HasSlideVelocity { get; }
    public bool JumpRequested { get; }
}
