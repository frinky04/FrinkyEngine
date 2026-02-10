using System.Numerics;
using FrinkyEngine.Core.ECS;
using FrinkyEngine.Core.Scene;
using FrinkyUi = FrinkyEngine.Core.UI.UI;
using FrinkyInput = FrinkyEngine.Core.Input.Input;
using Raylib_cs;

namespace FrinkyEngine.Core.Components;

/// <summary>
/// Simple configurable player input component for movement and mouse look.
/// Uses <see cref="CharacterControllerComponent"/> when present; otherwise falls back to transform or rigidbody motion.
/// </summary>
[ComponentCategory("Input")]
public class SimplePlayerInputComponent : Component
{
    private KeyboardKey _moveForwardKey = KeyboardKey.W;
    private KeyboardKey _moveBackwardKey = KeyboardKey.S;
    private KeyboardKey _moveLeftKey = KeyboardKey.A;
    private KeyboardKey _moveRightKey = KeyboardKey.D;
    private KeyboardKey _jumpKey = KeyboardKey.Space;
    private KeyboardKey _crouchKey = KeyboardKey.LeftControl;

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
    private bool _driveAttachedCamera = true;
    private Vector3 _attachedCameraLocalOffset = new(0f, 1.6f, 0f);
    private float _attachedCameraBackDistance = 0f;
    private EntityReference _cameraEntity;

    private bool _adjustCameraOnCrouch = true;
    private float _cameraOffsetLerpSpeed = 5f;
    private float _standingHeadHeight = 1.6f;

    private TransformComponent? _attachedCameraTransform;
    private float _lookYawDegrees;
    private float _lookPitchDegrees;
    private bool _lookInitialized;

    private Vector3 _originalCameraOffset;
    private bool _cameraOffsetCached;
    private float _currentCrouchCameraBlend;
    private float _cachedStandingCapsuleLength = -1f;
    private float _supportGracePeriod = 0f;
    private const float SupportGraceDuration = 0.1f; // 100ms grace period

    /// <summary>
    /// Key used to move forward. Defaults to <see cref="KeyboardKey.W"/>.
    /// </summary>
    [InspectorSection("Key Bindings")]
    [InspectorLabel("Forward")]
    [InspectorSearchableEnum]
    public KeyboardKey MoveForwardKey
    {
        get => _moveForwardKey;
        set => _moveForwardKey = value;
    }

    /// <summary>
    /// Key used to move backward. Defaults to <see cref="KeyboardKey.S"/>.
    /// </summary>
    [InspectorLabel("Backward")]
    [InspectorSearchableEnum]
    public KeyboardKey MoveBackwardKey
    {
        get => _moveBackwardKey;
        set => _moveBackwardKey = value;
    }

    /// <summary>
    /// Key used to strafe left. Defaults to <see cref="KeyboardKey.A"/>.
    /// </summary>
    [InspectorLabel("Left")]
    [InspectorSearchableEnum]
    public KeyboardKey MoveLeftKey
    {
        get => _moveLeftKey;
        set => _moveLeftKey = value;
    }

    /// <summary>
    /// Key used to strafe right. Defaults to <see cref="KeyboardKey.D"/>.
    /// </summary>
    [InspectorLabel("Right")]
    [InspectorSearchableEnum]
    public KeyboardKey MoveRightKey
    {
        get => _moveRightKey;
        set => _moveRightKey = value;
    }

    /// <summary>
    /// Key used for jumping. Defaults to <see cref="KeyboardKey.Space"/>.
    /// </summary>
    [InspectorLabel("Jump")]
    [InspectorSearchableEnum]
    public KeyboardKey JumpKey
    {
        get => _jumpKey;
        set => _jumpKey = value;
    }

    /// <summary>
    /// Enables mouse-driven look rotation.
    /// </summary>
    [InspectorSection("Mouse Look")]
    [InspectorLabel("Enable Mouse Look")]
    public bool EnableMouseLook
    {
        get => _enableMouseLook;
        set => _enableMouseLook = value;
    }

    /// <summary>
    /// If true, look rotation is only applied while <see cref="LookMouseButton"/> is held.
    /// </summary>
    [InspectorLabel("Require Mouse Button")]
    [InspectorVisibleIf(nameof(EnableMouseLook))]
    public bool RequireLookMouseButton
    {
        get => _requireLookMouseButton;
        set => _requireLookMouseButton = value;
    }

    /// <summary>
    /// Mouse button gate for look input when <see cref="RequireLookMouseButton"/> is enabled.
    /// </summary>
    [InspectorLabel("Look Mouse Button")]
    [InspectorSearchableEnum]
    [InspectorVisibleIf(nameof(EnableMouseLook))]
    [InspectorVisibleIf(nameof(RequireLookMouseButton))]
    public MouseButton LookMouseButton
    {
        get => _lookMouseButton;
        set => _lookMouseButton = value;
    }

    /// <summary>
    /// If true, applies pitch rotation around the local X axis.
    /// </summary>
    [InspectorVisibleIf(nameof(EnableMouseLook))]
    public bool RotatePitch
    {
        get => _rotatePitch;
        set => _rotatePitch = value;
    }

    /// <summary>
    /// If true, character look uses <see cref="CharacterControllerComponent.ViewDirectionOverride"/>
    /// so pitch does not have to rotate the physics body.
    /// </summary>
    [InspectorLabel("Use View Override")]
    [InspectorVisibleIf(nameof(EnableMouseLook))]
    public bool UseViewDirectionOverrideForCharacterLook
    {
        get => _useViewDirectionOverrideForCharacterLook;
        set => _useViewDirectionOverrideForCharacterLook = value;
    }

    /// <summary>
    /// If true and a character controller is active, pitch is applied to the entity transform.
    /// Keeping this false avoids tilting the character body.
    /// </summary>
    [InspectorLabel("Apply Pitch To Body")]
    [InspectorVisibleIf(nameof(EnableMouseLook))]
    public bool ApplyPitchToCharacterBody
    {
        get => _applyPitchToCharacterBody;
        set => _applyPitchToCharacterBody = value;
    }

    /// <summary>
    /// Inverts mouse Y input for pitch rotation.
    /// </summary>
    [InspectorLabel("Invert Y")]
    [InspectorVisibleIf(nameof(EnableMouseLook))]
    public bool InvertMouseY
    {
        get => _invertMouseY;
        set => _invertMouseY = value;
    }

    /// <summary>
    /// Inverts mouse X input for yaw rotation.
    /// </summary>
    [InspectorLabel("Invert X")]
    [InspectorVisibleIf(nameof(EnableMouseLook))]
    public bool InvertMouseX
    {
        get => _invertMouseX;
        set => _invertMouseX = value;
    }

    /// <summary>
    /// Sensitivity multiplier applied to mouse delta.
    /// </summary>
    [InspectorLabel("Sensitivity")]
    [InspectorVisibleIf(nameof(EnableMouseLook))]
    public float MouseSensitivity
    {
        get => _mouseSensitivity;
        set => _mouseSensitivity = float.IsFinite(value) ? MathF.Max(0f, value) : 0.1f;
    }

    /// <summary>
    /// Lower clamp for pitch in degrees.
    /// </summary>
    [InspectorLabel("Min Pitch")]
    [InspectorVisibleIf(nameof(EnableMouseLook))]
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
    [InspectorLabel("Max Pitch")]
    [InspectorVisibleIf(nameof(EnableMouseLook))]
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
    [InspectorSection("Character Controller")]
    [InspectorLabel("Use Character Controller")]
    public bool UseCharacterController
    {
        get => _useCharacterController;
        set => _useCharacterController = value;
    }

    /// <summary>
    /// Allows jump key processing.
    /// </summary>
    [InspectorLabel("Allow Jump")]
    public bool AllowJump
    {
        get => _allowJump;
        set => _allowJump = value;
    }

    /// <summary>
    /// Speed used by fallback motion when no character controller is present.
    /// </summary>
    [InspectorSection("Fallback Motion")]
    [InspectorLabel("Move Speed")]
    [InspectorVisibleIf(nameof(UseCharacterController), false)]
    public float FallbackMoveSpeed
    {
        get => _fallbackMoveSpeed;
        set => _fallbackMoveSpeed = float.IsFinite(value) ? MathF.Max(0f, value) : 4f;
    }

    /// <summary>
    /// Upward impulse used for fallback rigidbody jumping.
    /// </summary>
    [InspectorLabel("Jump Impulse")]
    [InspectorVisibleIf(nameof(UseCharacterController), false)]
    public float FallbackJumpImpulse
    {
        get => _fallbackJumpImpulse;
        set => _fallbackJumpImpulse = float.IsFinite(value) ? MathF.Max(0f, value) : 3f;
    }

    /// <summary>
    /// If true, a child camera entity is driven using janky follow/pitch behavior.
    /// </summary>
    [InspectorSection("Attached Camera")]
    [InspectorLabel("Drive Attached Camera")]
    public bool DriveAttachedCamera
    {
        get => _driveAttachedCamera;
        set => _driveAttachedCamera = value;
    }

    /// <summary>
    /// Local offset applied to the attached camera entity relative to the controller entity.
    /// </summary>
    [InspectorLabel("Local Offset")]
    [InspectorVisibleIf(nameof(DriveAttachedCamera))]
    public Vector3 AttachedCameraLocalOffset
    {
        get => _attachedCameraLocalOffset;
        set => _attachedCameraLocalOffset = value;
    }

    /// <summary>
    /// Additional camera distance along local +Z (behind the entity when forward is -Z).
    /// </summary>
    [InspectorLabel("Back Distance")]
    [InspectorVisibleIf(nameof(DriveAttachedCamera))]
    public float AttachedCameraBackDistance
    {
        get => _attachedCameraBackDistance;
        set => _attachedCameraBackDistance = float.IsFinite(value) ? value : 0f;
    }

    /// <summary>
    /// Optional explicit reference to the camera entity. When set, takes priority over child entity search.
    /// </summary>
    [InspectorLabel("Camera Entity")]
    [InspectorVisibleIf(nameof(DriveAttachedCamera))]
    public EntityReference CameraEntity
    {
        get => _cameraEntity;
        set => _cameraEntity = value;
    }

    /// <summary>
    /// Key used to crouch. Defaults to <see cref="KeyboardKey.LeftControl"/>.
    /// </summary>
    public KeyboardKey CrouchKey
    {
        get => _crouchKey;
        set => _crouchKey = value;
    }

    /// <summary>
    /// If true, automatically adjusts <see cref="AttachedCameraLocalOffset"/> when crouching.
    /// </summary>
    public bool AdjustCameraOnCrouch
    {
        get => _adjustCameraOnCrouch;
        set => _adjustCameraOnCrouch = value;
    }

    /// <summary>
    /// Speed of camera offset interpolation during crouch transitions.
    /// Higher values = faster transition. Default is 5.0.
    /// </summary>
    public float CameraOffsetLerpSpeed
    {
        get => _cameraOffsetLerpSpeed;
        set => _cameraOffsetLerpSpeed = MathF.Max(0.1f, value);
    }

    /// <summary>
    /// Camera height from feet when standing (in meters).
    /// Crouching height is automatically calculated using the character controller's crouch scale.
    /// Default is 1.6.
    /// </summary>
    public float StandingHeadHeight
    {
        get => _standingHeadHeight;
        set => _standingHeadHeight = MathF.Max(0.1f, value);
    }

    /// <inheritdoc />
    public override void Start()
    {
        InitializeLookState();
        ResolveAttachedCamera();

        // Cache original camera offset for crouch adjustments
        if (!_cameraOffsetCached)
        {
            _originalCameraOffset = _attachedCameraLocalOffset;
            _cameraOffsetCached = true;
        }

        // Reset cached capsule length when component starts
        _cachedStandingCapsuleLength = -1f;
    }

    /// <inheritdoc />
    public override void Update(float dt)
    {
        if (!float.IsFinite(dt) || dt <= 0f)
            return;

        var uiCapture = FrinkyUi.InputCapture;
        bool blockMouseLook = uiCapture.WantsMouse;
        bool blockKeyboardInput = uiCapture.WantsKeyboard || uiCapture.WantsTextInput;

        CharacterControllerComponent? controller = null;
        if (UseCharacterController)
        {
            var candidate = Entity.GetComponent<CharacterControllerComponent>();
            if (candidate is { Enabled: true })
                controller = candidate;
        }

        ApplyMouseLook(controller, blockMouseLook);

        // Handle crouch input
        if (!blockKeyboardInput && controller != null)
        {
            bool crouchKeyDown = FrinkyInput.IsKeyDown(CrouchKey);
            controller.SetCrouching(crouchKeyDown);
        }

        // Apply camera offset adjustments for crouch
        if (AdjustCameraOnCrouch && controller != null)
        {
            UpdateCrouchCameraBlend(controller, dt);
        }

        UpdateAttachedCamera(controller);

        // Update support grace period for camera smoothing
        if (controller != null)
        {
            UpdateSupportGracePeriod(controller, dt);
        }

        var moveInput = blockKeyboardInput ? Vector2.Zero : ReadMoveInput();

        if (controller != null)
        {
            controller.SetMoveInput(moveInput);
            if (!blockKeyboardInput && AllowJump && FrinkyInput.IsKeyPressed(JumpKey))
                controller.Jump();
            return;
        }

        ApplyFallbackMovement(moveInput, dt, !blockKeyboardInput);
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

    private void ApplyMouseLook(CharacterControllerComponent? controller, bool blockMouseLook)
    {
        if (!EnableMouseLook)
            return;
        if (blockMouseLook)
            return;

        if (RequireLookMouseButton && !FrinkyInput.IsMouseButtonDown(LookMouseButton))
            return;

        InitializeLookState();

        var delta = FrinkyInput.MouseDelta;
        var yawSign = InvertMouseX ? 1f : -1f;
        _lookYawDegrees += delta.X * MouseSensitivity * yawSign;
        _lookYawDegrees = WrapDegrees(_lookYawDegrees);

        if (RotatePitch)
        {
            var pitchSign = InvertMouseY ? 1f : -1f;
            var nextPitch = _lookPitchDegrees + delta.Y * MouseSensitivity * pitchSign;
            _lookPitchDegrees = Math.Clamp(nextPitch, MinPitchDegrees, MaxPitchDegrees);
        }

        if (controller != null && UseViewDirectionOverrideForCharacterLook)
        {
            var bodyPitch = ApplyPitchToCharacterBody && RotatePitch ? _lookPitchDegrees : 0f;
            Entity.Transform.LocalRotation = BuildYawPitchRotation(_lookYawDegrees, bodyPitch);

            if (controller.UseEntityForwardAsViewDirection)
                controller.UseEntityForwardAsViewDirection = false;

            controller.ViewDirectionOverride = BuildViewDirection(_lookYawDegrees, _lookPitchDegrees);
            return;
        }

        var fallbackPitch = RotatePitch ? _lookPitchDegrees : 0f;
        Entity.Transform.LocalRotation = BuildYawPitchRotation(_lookYawDegrees, fallbackPitch);
    }

    private void InitializeLookState()
    {
        if (_lookInitialized)
            return;

        ResolveAttachedCamera();

        var euler = Entity.Transform.EulerAngles;
        _lookYawDegrees = euler.Y;
        var initialPitch = euler.X;
        if (_attachedCameraTransform != null)
            initialPitch = _attachedCameraTransform.EulerAngles.X;
        _lookPitchDegrees = Math.Clamp(initialPitch, MinPitchDegrees, MaxPitchDegrees);
        _lookInitialized = true;
    }

    private static Vector3 BuildViewDirection(float yawDegrees, float pitchDegrees)
    {
        var rotation = BuildYawPitchRotation(yawDegrees, pitchDegrees);
        var forward = Vector3.Transform(-Vector3.UnitZ, rotation);
        return SafeNormalize(forward, -Vector3.UnitZ);
    }

    private static Quaternion BuildYawPitchRotation(float yawDegrees, float pitchDegrees)
    {
        var yawRadians = DegreesToRadians(yawDegrees);
        var pitchRadians = DegreesToRadians(pitchDegrees);
        return Quaternion.Normalize(Quaternion.CreateFromYawPitchRoll(yawRadians, pitchRadians, 0f));
    }

    private void ResolveAttachedCamera()
    {
        if (_attachedCameraTransform != null &&
            _attachedCameraTransform.Entity.Scene == Entity.Scene)
        {
            return;
        }

        // Try explicit EntityReference first
        if (_cameraEntity.IsValid)
        {
            var referenced = _cameraEntity.Resolve(Entity);
            if (referenced != null)
            {
                var cam = referenced.GetComponent<CameraComponent>();
                if (cam is { Enabled: true })
                {
                    _attachedCameraTransform = referenced.Transform;
                    return;
                }
            }
        }

        // Fall back to child entity search
        _attachedCameraTransform = FindAttachedCameraTransform(Entity.Transform);
    }

    private void UpdateAttachedCamera(CharacterControllerComponent? controller)
    {
        if (!DriveAttachedCamera)
            return;

        ResolveAttachedCamera();
        if (_attachedCameraTransform == null)
            return;

        var localPosition = AttachedCameraLocalOffset + new Vector3(0f, 0f, AttachedCameraBackDistance);
        _attachedCameraTransform.LocalPosition = localPosition;

        float appliedPitch = 0f;
        if (RotatePitch && (!ApplyPitchToCharacterBody || controller == null))
            appliedPitch = _lookPitchDegrees;

        _attachedCameraTransform.WorldRotation = BuildYawPitchRotation(_lookYawDegrees, appliedPitch);
    }

    private static TransformComponent? FindAttachedCameraTransform(TransformComponent root)
    {
        foreach (var child in root.Children)
        {
            if (child.Entity.GetComponent<CameraComponent>() is { Enabled: true })
                return child;

            var nested = FindAttachedCameraTransform(child);
            if (nested != null)
                return nested;
        }

        return null;
    }

    private void ApplyFallbackMovement(Vector2 moveInput, float dt, bool allowJumpInput)
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

            if (allowJumpInput && AllowJump && FrinkyInput.IsKeyPressed(JumpKey))
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

    private static float DegreesToRadians(float degrees)
    {
        return degrees * (MathF.PI / 180f);
    }

    private static float WrapDegrees(float degrees)
    {
        var wrapped = degrees % 360f;
        if (wrapped > 180f)
            wrapped -= 360f;
        if (wrapped < -180f)
            wrapped += 360f;
        return wrapped;
    }

    private void UpdateCrouchCameraBlend(CharacterControllerComponent controller, float dt)
    {
        if (!_cameraOffsetCached)
            return;

        var capsule = Entity.GetComponent<CapsuleColliderComponent>();
        if (capsule == null || !capsule.Enabled)
            return;

        // Cache standing capsule length on first use
        if (_cachedStandingCapsuleLength < 0f && !controller.IsCrouching)
        {
            _cachedStandingCapsuleLength = capsule.Length;
        }

        // If we haven't cached yet (started crouched), use current length as estimate
        if (_cachedStandingCapsuleLength < 0f)
        {
            _cachedStandingCapsuleLength = capsule.Length / controller.CrouchHeightScale;
        }

        // Interpolate crouch blend
        float targetBlend = controller.IsCrouching ? 1.0f : 0.0f;
        if (MathF.Abs(_currentCrouchCameraBlend - targetBlend) > 0.001f)
        {
            _currentCrouchCameraBlend = Lerp(_currentCrouchCameraBlend, targetBlend, _cameraOffsetLerpSpeed * dt);
        }
        else
        {
            _currentCrouchCameraBlend = targetBlend;
        }

        float capsuleRadius = capsule.Radius;

        // Use blended capsule length only when in air (not supported)
        // When grounded, the position compensation in ApplyCrouchToCapsule handles smoothing
        float capsuleLength;
        if (IsEffectivelyGrounded(controller))
        {
            // Grounded: use actual capsule length (position compensation handles smoothing)
            capsuleLength = capsule.Length;
        }
        else
        {
            // In air: use virtual blended length to prevent snap
            float crouchedCapsuleLength = _cachedStandingCapsuleLength * controller.CrouchHeightScale;
            capsuleLength = Lerp(_cachedStandingCapsuleLength, crouchedCapsuleLength, _currentCrouchCameraBlend);
        }

        // Calculate camera position
        float capsuleBottom = -(capsuleLength * 0.5f + capsuleRadius);

        // Calculate crouching head height using controller's crouch scale
        float crouchingHeadHeight = _standingHeadHeight * controller.CrouchHeightScale;

        // Lerp head height based on crouch blend
        float headHeight = Lerp(_standingHeadHeight, crouchingHeadHeight, _currentCrouchCameraBlend);

        float cameraY = capsuleBottom + headHeight;

        _attachedCameraLocalOffset = new Vector3(
            _originalCameraOffset.X,
            cameraY,
            _originalCameraOffset.Z
        );
    }

    private static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * Math.Clamp(t, 0f, 1f);
    }

    /// <summary>
    /// Updates the support grace period timer for camera smoothing.
    /// Grace period prevents camera jitter from 1-frame support losses during crouch transitions.
    /// </summary>
    private void UpdateSupportGracePeriod(CharacterControllerComponent controller, float dt)
    {
        if (controller.Supported)
        {
            // Reset grace period when truly supported
            _supportGracePeriod = 0f;
        }
        else if (_supportGracePeriod < SupportGraceDuration)
        {
            // Count up grace period while unsupported
            _supportGracePeriod += dt;
        }
    }

    /// <summary>
    /// Determines if the character should be treated as grounded for camera calculations.
    /// Returns true if truly supported, or within grace period after losing support.
    /// </summary>
    private bool IsEffectivelyGrounded(CharacterControllerComponent controller)
    {
        return controller.Supported || _supportGracePeriod < SupportGraceDuration;
    }
}
