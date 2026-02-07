using System.Numerics;
using FrinkyEngine.Core.ECS;
using FrinkyInput = FrinkyEngine.Core.Input.Input;
using Raylib_cs;

namespace FrinkyEngine.Core.Components;

/// <summary>
/// Simple configurable player input component for movement and mouse look.
/// Uses <see cref="CharacterControllerComponent"/> when present; otherwise falls back to transform or rigidbody motion.
/// </summary>
public class SimplePlayerInputComponent : Component
{
    private KeyboardKey _moveForwardKey = KeyboardKey.W;
    private KeyboardKey _moveBackwardKey = KeyboardKey.S;
    private KeyboardKey _moveLeftKey = KeyboardKey.A;
    private KeyboardKey _moveRightKey = KeyboardKey.D;
    private KeyboardKey _jumpKey = KeyboardKey.Space;

    private bool _enableMouseLook = true;
    private bool _requireLookMouseButton;
    private MouseButton _lookMouseButton = MouseButton.Right;
    private bool _rotatePitch = true;
    private bool _useViewDirectionOverrideForCharacterLook = true;
    private bool _applyPitchToCharacterBody;
    private bool _invertMouseX;
    private bool _invertMouseY;
    private float _mouseSensitivity = 0.1f;
    private float _minPitchDegrees = -85f;
    private float _maxPitchDegrees = 85f;

    private bool _useCharacterController = true;
    private bool _allowJump = true;
    private float _fallbackMoveSpeed = 4f;
    private float _fallbackJumpImpulse = 3f;
    private float _lookYawDegrees;
    private float _lookPitchDegrees;
    private bool _lookInitialized;

    /// <summary>
    /// Key used to move forward. Defaults to <see cref="KeyboardKey.W"/>.
    /// </summary>
    public KeyboardKey MoveForwardKey
    {
        get => _moveForwardKey;
        set => _moveForwardKey = value;
    }

    /// <summary>
    /// Key used to move backward. Defaults to <see cref="KeyboardKey.S"/>.
    /// </summary>
    public KeyboardKey MoveBackwardKey
    {
        get => _moveBackwardKey;
        set => _moveBackwardKey = value;
    }

    /// <summary>
    /// Key used to strafe left. Defaults to <see cref="KeyboardKey.A"/>.
    /// </summary>
    public KeyboardKey MoveLeftKey
    {
        get => _moveLeftKey;
        set => _moveLeftKey = value;
    }

    /// <summary>
    /// Key used to strafe right. Defaults to <see cref="KeyboardKey.D"/>.
    /// </summary>
    public KeyboardKey MoveRightKey
    {
        get => _moveRightKey;
        set => _moveRightKey = value;
    }

    /// <summary>
    /// Key used for jumping. Defaults to <see cref="KeyboardKey.Space"/>.
    /// </summary>
    public KeyboardKey JumpKey
    {
        get => _jumpKey;
        set => _jumpKey = value;
    }

    /// <summary>
    /// Enables mouse-driven look rotation.
    /// </summary>
    public bool EnableMouseLook
    {
        get => _enableMouseLook;
        set => _enableMouseLook = value;
    }

    /// <summary>
    /// If true, look rotation is only applied while <see cref="LookMouseButton"/> is held.
    /// </summary>
    public bool RequireLookMouseButton
    {
        get => _requireLookMouseButton;
        set => _requireLookMouseButton = value;
    }

    /// <summary>
    /// Mouse button gate for look input when <see cref="RequireLookMouseButton"/> is enabled.
    /// </summary>
    public MouseButton LookMouseButton
    {
        get => _lookMouseButton;
        set => _lookMouseButton = value;
    }

    /// <summary>
    /// If true, applies pitch rotation around the local X axis.
    /// </summary>
    public bool RotatePitch
    {
        get => _rotatePitch;
        set => _rotatePitch = value;
    }

    /// <summary>
    /// If true, character look uses <see cref="CharacterControllerComponent.ViewDirectionOverride"/>
    /// so pitch does not have to rotate the physics body.
    /// </summary>
    public bool UseViewDirectionOverrideForCharacterLook
    {
        get => _useViewDirectionOverrideForCharacterLook;
        set => _useViewDirectionOverrideForCharacterLook = value;
    }

    /// <summary>
    /// If true and a character controller is active, pitch is applied to the entity transform.
    /// Keeping this false avoids tilting the character body.
    /// </summary>
    public bool ApplyPitchToCharacterBody
    {
        get => _applyPitchToCharacterBody;
        set => _applyPitchToCharacterBody = value;
    }

    /// <summary>
    /// Inverts mouse Y input for pitch rotation.
    /// </summary>
    public bool InvertMouseY
    {
        get => _invertMouseY;
        set => _invertMouseY = value;
    }

    /// <summary>
    /// Inverts mouse X input for yaw rotation.
    /// </summary>
    public bool InvertMouseX
    {
        get => _invertMouseX;
        set => _invertMouseX = value;
    }

    /// <summary>
    /// Sensitivity multiplier applied to mouse delta.
    /// </summary>
    public float MouseSensitivity
    {
        get => _mouseSensitivity;
        set => _mouseSensitivity = float.IsFinite(value) ? MathF.Max(0f, value) : 0.1f;
    }

    /// <summary>
    /// Lower clamp for pitch in degrees.
    /// </summary>
    public float MinPitchDegrees
    {
        get => _minPitchDegrees;
        set
        {
            var clamped = float.IsFinite(value) ? Math.Clamp(value, -89f, 89f) : -85f;
            _minPitchDegrees = MathF.Min(clamped, _maxPitchDegrees);
        }
    }

    /// <summary>
    /// Upper clamp for pitch in degrees.
    /// </summary>
    public float MaxPitchDegrees
    {
        get => _maxPitchDegrees;
        set
        {
            var clamped = float.IsFinite(value) ? Math.Clamp(value, -89f, 89f) : 85f;
            _maxPitchDegrees = MathF.Max(clamped, _minPitchDegrees);
        }
    }

    /// <summary>
    /// If true, forwards movement and jump to <see cref="CharacterControllerComponent"/> when available.
    /// </summary>
    public bool UseCharacterController
    {
        get => _useCharacterController;
        set => _useCharacterController = value;
    }

    /// <summary>
    /// Allows jump key processing.
    /// </summary>
    public bool AllowJump
    {
        get => _allowJump;
        set => _allowJump = value;
    }

    /// <summary>
    /// Speed used by fallback motion when no character controller is present.
    /// </summary>
    public float FallbackMoveSpeed
    {
        get => _fallbackMoveSpeed;
        set => _fallbackMoveSpeed = float.IsFinite(value) ? MathF.Max(0f, value) : 4f;
    }

    /// <summary>
    /// Upward impulse used for fallback rigidbody jumping.
    /// </summary>
    public float FallbackJumpImpulse
    {
        get => _fallbackJumpImpulse;
        set => _fallbackJumpImpulse = float.IsFinite(value) ? MathF.Max(0f, value) : 3f;
    }

    /// <inheritdoc />
    public override void Start()
    {
        InitializeLookState();
    }

    /// <inheritdoc />
    public override void Update(float dt)
    {
        if (!float.IsFinite(dt) || dt <= 0f)
            return;

        CharacterControllerComponent? controller = null;
        if (UseCharacterController)
        {
            var candidate = Entity.GetComponent<CharacterControllerComponent>();
            if (candidate is { Enabled: true })
                controller = candidate;
        }

        ApplyMouseLook(controller);

        var moveInput = ReadMoveInput();

        if (controller != null)
        {
            controller.SetMoveInput(moveInput);
            if (AllowJump && FrinkyInput.IsKeyPressed(JumpKey))
                controller.Jump();
            return;
        }

        ApplyFallbackMovement(moveInput, dt);
    }

    private Vector2 ReadMoveInput()
    {
        var input = Vector2.Zero;

        if (FrinkyInput.IsKeyDown(MoveForwardKey))
            input.Y += 1f;
        if (FrinkyInput.IsKeyDown(MoveBackwardKey))
            input.Y -= 1f;
        if (FrinkyInput.IsKeyDown(MoveLeftKey))
            input.X -= 1f;
        if (FrinkyInput.IsKeyDown(MoveRightKey))
            input.X += 1f;

        var lengthSquared = input.LengthSquared();
        if (lengthSquared > 1f)
            input /= MathF.Sqrt(lengthSquared);

        return input;
    }

    private void ApplyMouseLook(CharacterControllerComponent? controller)
    {
        if (!EnableMouseLook)
            return;

        if (RequireLookMouseButton && !FrinkyInput.IsMouseButtonDown(LookMouseButton))
            return;

        InitializeLookState();

        var delta = FrinkyInput.MouseDelta;
        var yawSign = InvertMouseX ? 1f : -1f;
        _lookYawDegrees += delta.X * MouseSensitivity * yawSign;

        if (RotatePitch)
        {
            var pitchSign = InvertMouseY ? 1f : -1f;
            var nextPitch = _lookPitchDegrees + delta.Y * MouseSensitivity * pitchSign;
            _lookPitchDegrees = Math.Clamp(nextPitch, MinPitchDegrees, MaxPitchDegrees);
        }

        if (controller != null && UseViewDirectionOverrideForCharacterLook)
        {
            var euler = Entity.Transform.EulerAngles;
            euler.Y = _lookYawDegrees;
            if (ApplyPitchToCharacterBody && RotatePitch)
                euler.X = _lookPitchDegrees;
            Entity.Transform.EulerAngles = euler;

            if (controller.UseEntityForwardAsViewDirection)
                controller.UseEntityForwardAsViewDirection = false;

            controller.ViewDirectionOverride = BuildViewDirection(_lookYawDegrees, _lookPitchDegrees);
            return;
        }

        var fallbackEuler = Entity.Transform.EulerAngles;
        fallbackEuler.Y = _lookYawDegrees;
        if (RotatePitch)
            fallbackEuler.X = _lookPitchDegrees;
        Entity.Transform.EulerAngles = fallbackEuler;
    }

    private void InitializeLookState()
    {
        if (_lookInitialized)
            return;

        var euler = Entity.Transform.EulerAngles;
        _lookYawDegrees = euler.Y;
        _lookPitchDegrees = Math.Clamp(euler.X, MinPitchDegrees, MaxPitchDegrees);
        _lookInitialized = true;
    }

    private static Vector3 BuildViewDirection(float yawDegrees, float pitchDegrees)
    {
        var yawRadians = yawDegrees * (MathF.PI / 180f);
        var pitchRadians = pitchDegrees * (MathF.PI / 180f);
        var rotation = Quaternion.CreateFromYawPitchRoll(yawRadians, pitchRadians, 0f);
        var forward = Vector3.Transform(-Vector3.UnitZ, rotation);
        return SafeNormalize(forward, -Vector3.UnitZ);
    }

    private void ApplyFallbackMovement(Vector2 moveInput, float dt)
    {
        var transform = Entity.Transform;
        var forward = transform.Forward;
        forward.Y = 0f;
        forward = SafeNormalize(forward, -Vector3.UnitZ);

        var right = transform.Right;
        right.Y = 0f;
        right = SafeNormalize(right, Vector3.UnitX);

        var desiredVelocity = (right * moveInput.X + forward * moveInput.Y) * FallbackMoveSpeed;

        var rigidbody = Entity.GetComponent<RigidbodyComponent>();
        if (rigidbody is { Enabled: true } && rigidbody.MotionType == BodyMotionType.Dynamic)
        {
            var currentVelocity = rigidbody.GetLinearVelocity();
            currentVelocity.X = desiredVelocity.X;
            currentVelocity.Z = desiredVelocity.Z;
            rigidbody.SetLinearVelocity(currentVelocity);

            if (AllowJump && FrinkyInput.IsKeyPressed(JumpKey))
                rigidbody.ApplyImpulse(Vector3.UnitY * FallbackJumpImpulse);

            return;
        }

        transform.LocalPosition += desiredVelocity * dt;
    }

    private static Vector3 SafeNormalize(Vector3 value, Vector3 fallback)
    {
        var lengthSquared = value.LengthSquared();
        if (lengthSquared <= 1e-12f)
            return fallback;

        return value / MathF.Sqrt(lengthSquared);
    }
}
