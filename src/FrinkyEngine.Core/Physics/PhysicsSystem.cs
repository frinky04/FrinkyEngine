using System.Diagnostics;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Constraints;
using BepuUtilities.Memory;
using FrinkyEngine.Core.Components;
using FrinkyEngine.Core.ECS;
using FrinkyEngine.Core.Physics.Characters;
using FrinkyEngine.Core.Rendering;

namespace FrinkyEngine.Core.Physics;

internal sealed class PhysicsSystem : IDisposable
{
    private sealed class PhysicsBodyState
    {
        public required Entity Entity;
        public required RigidbodyComponent Rigidbody;
        public required ColliderComponent Collider;
        public BodyHandle? BodyHandle;
        public StaticHandle? StaticHandle;
        public int ConfigurationHash;
        public Vector3 LockedPosition;
        public Quaternion LockReferenceOrientation;
        public byte RotationLockMask;
        public Vector3 AuthoritativeLocalPosition;
        public Quaternion AuthoritativeLocalRotation;
        public Vector3 LastPublishedVisualLocalPosition;
        public Quaternion LastPublishedVisualLocalRotation;
        public bool HasPublishedVisualPose;
        public RigidPose PreviousSimulationPose;
        public RigidPose CurrentSimulationPose;
        public bool HasSimulationPoseHistory;
        public bool SuppressInterpolationForFrame;
        public RigidPose PreviousKinematicTargetPose;
        public bool HasPreviousKinematicTargetPose;
    }

    private readonly struct ShapeCacheKey : IEquatable<ShapeCacheKey>
    {
        private readonly byte _shapeType;
        private readonly float _a;
        private readonly float _b;
        private readonly float _c;

        private ShapeCacheKey(byte shapeType, float a, float b, float c)
        {
            _shapeType = shapeType;
            _a = a;
            _b = b;
            _c = c;
        }

        public static ShapeCacheKey Box(float x, float y, float z) => new(0, x, y, z);
        public static ShapeCacheKey Sphere(float radius) => new(1, radius, 0f, 0f);
        public static ShapeCacheKey Capsule(float radius, float length) => new(2, radius, length, 0f);

        public bool Equals(ShapeCacheKey other)
        {
            return _shapeType == other._shapeType &&
                   _a.Equals(other._a) &&
                   _b.Equals(other._b) &&
                   _c.Equals(other._c);
        }

        public override bool Equals(object? obj)
        {
            return obj is ShapeCacheKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_shapeType, _a, _b, _c);
        }
    }

    private readonly Scene.Scene _scene;
    private readonly BufferPool _bufferPool = new();
    private readonly PhysicsMaterialTable _materialTable = new();
    private readonly Dictionary<ShapeCacheKey, TypedIndex> _shapeCache = new();
    private readonly Dictionary<Guid, PhysicsBodyState> _bodyStates = new();
    private readonly Dictionary<Guid, CharacterControllerRuntimeState> _characterStates = new();
    private readonly HashSet<Guid> _warnedNoCollider = new();
    private readonly HashSet<Guid> _warnedParented = new();
    private readonly HashSet<Guid> _warnedMultipleColliders = new();
    private readonly HashSet<Guid> _warnedMultipleRigidbodies = new();
    private readonly HashSet<Guid> _warnedCharacterMissingRigidbody = new();
    private readonly HashSet<Guid> _warnedCharacterMissingCapsule = new();
    private readonly HashSet<Guid> _warnedCharacterWrongMotionType = new();
    private readonly HashSet<Guid> _warnedCharacterParented = new();
    private readonly HashSet<Guid> _warnedCharacterNonCapsuleBody = new();
    private readonly HashSet<Guid> _warnedKinematicDiscontinuity = new();
    private readonly CharacterControllerBridge _characterBridge = new();

    private Simulation? _simulation;
    private CharacterControllers? _characterControllers;
    private float _accumulator;
    private int _lastSubstepCount;
    private double _lastStepTimeMs;
    private const byte RotationLockXMask = 1 << 0;
    private const byte RotationLockYMask = 1 << 1;
    private const byte RotationLockZMask = 1 << 2;
    private const float MaxKinematicLinearSpeed = 50f;
    private const float MaxKinematicAngularSpeed = 20f;
    private const float MaxKinematicAngularStepDegrees = 120f;
    private const float KinematicDiscontinuityLinearSpeedMultiplier = 2f;

    public PhysicsSystem(Scene.Scene scene)
    {
        _scene = scene;
    }

    public bool IsInitialized => _simulation != null;

    public void Initialize()
    {
        if (_simulation != null)
            return;

        _scene.PhysicsSettings.Normalize();
        var projSettings = PhysicsProjectSettings.Current;
        projSettings.Normalize();

        _characterControllers = new CharacterControllers(_bufferPool);
        var narrowPhaseCallbacks = new PhysicsNarrowPhaseCallbacks(
            new SpringSettings(projSettings.ContactSpringFrequency, projSettings.ContactDampingRatio),
            projSettings.MaximumRecoveryVelocity,
            projSettings.DefaultFriction,
            projSettings.DefaultRestitution,
            _materialTable,
            _characterControllers);
        var poseCallbacks = new PhysicsPoseIntegratorCallbacks(_scene.PhysicsSettings.Gravity);
        var solveDescription = new SolveDescription(projSettings.SolverVelocityIterations, projSettings.SolverSubsteps);

        _simulation = Simulation.Create(_bufferPool, narrowPhaseCallbacks, poseCallbacks, solveDescription);
        _accumulator = 0f;

        ReconcileParticipants();
    }

    public void Step(float dt)
    {
        if (_simulation == null)
            return;

        if (!float.IsFinite(dt) || dt <= 0f)
            return;

        var sw = Stopwatch.StartNew();

        ReconcileParticipants();

        var projSettings = PhysicsProjectSettings.Current;
        projSettings.Normalize();
        var fixedDt = projSettings.FixedTimestep;
        var maxSubsteps = projSettings.MaxSubstepsPerFrame;

        _accumulator += dt;
        int steps = 0;
        _characterBridge.CaptureFrameInput(_characterStates.Values);

        while (_accumulator >= fixedDt && steps < maxSubsteps)
        {
            PushSceneTransformsToPhysics(fixedDt);
            ApplyPendingForces(fixedDt);
            ApplyCharacterGoalsForStep(fixedDt, allowJump: steps == 0);
            _simulation.Timestep(fixedDt);
            ApplyPostStepBodyModifiers(fixedDt);
            SyncDynamicTransformsFromPhysics();
            SyncCharacterRuntimeState();

            _accumulator -= fixedDt;
            steps++;
        }

        if (steps > 0)
            _characterBridge.ConsumeFrameInput(_characterStates.Values);

        if (steps >= maxSubsteps && _accumulator >= fixedDt)
            _accumulator = 0f;

        sw.Stop();
        _lastSubstepCount = steps;
        _lastStepTimeMs = sw.Elapsed.TotalMilliseconds;
    }

    public PhysicsFrameStats GetFrameStats()
    {
        if (_simulation == null)
            return default;

        int dynamic = 0, kinematic = 0, staticCount = 0;
        foreach (var state in _bodyStates.Values)
        {
            switch (state.Rigidbody.MotionType)
            {
                case BodyMotionType.Dynamic: dynamic++; break;
                case BodyMotionType.Kinematic: kinematic++; break;
                case BodyMotionType.Static: staticCount++; break;
            }
        }

        return new PhysicsFrameStats(
            Valid: true,
            DynamicBodies: dynamic,
            KinematicBodies: kinematic,
            StaticBodies: staticCount,
            SubstepsThisFrame: _lastSubstepCount,
            StepTimeMs: _lastStepTimeMs,
            ActiveCharacterControllers: _characterStates.Count);
    }

    public void OnComponentStateChanged()
    {
        // Reconciliation runs each frame; this hook exists so components can signal immediate intent.
    }

    public void OnEntityRemoved(Entity entity)
    {
        if (_simulation == null)
            return;

        if (_bodyStates.TryGetValue(entity.Id, out var state))
        {
            RemoveBodyState(state);
            _bodyStates.Remove(entity.Id);
        }

        RemoveCharacterStateIfPresent(entity.Id);

        _warnedNoCollider.Remove(entity.Id);
        _warnedParented.Remove(entity.Id);
        _warnedMultipleColliders.Remove(entity.Id);
        _warnedMultipleRigidbodies.Remove(entity.Id);
        _warnedCharacterMissingRigidbody.Remove(entity.Id);
        _warnedCharacterMissingCapsule.Remove(entity.Id);
        _warnedCharacterWrongMotionType.Remove(entity.Id);
        _warnedCharacterParented.Remove(entity.Id);
        _warnedCharacterNonCapsuleBody.Remove(entity.Id);
        _warnedKinematicDiscontinuity.Remove(entity.Id);
    }

    public bool TryGetLinearVelocity(RigidbodyComponent rigidbody, out Vector3 velocity)
    {
        velocity = Vector3.Zero;
        if (_simulation == null)
            return false;

        if (!_bodyStates.TryGetValue(rigidbody.Entity.Id, out var state))
            return false;
        if (state.BodyHandle is not BodyHandle handle)
            return false;
        if (!_simulation.Bodies.BodyExists(handle))
            return false;

        var body = _simulation.Bodies.GetBodyReference(handle);
        velocity = body.Velocity.Linear;
        return true;
    }

    public void SetLinearVelocity(RigidbodyComponent rigidbody, Vector3 velocity)
    {
        if (_simulation == null)
            return;

        if (!_bodyStates.TryGetValue(rigidbody.Entity.Id, out var state))
            return;
        if (state.BodyHandle is not BodyHandle handle)
            return;
        if (!_simulation.Bodies.BodyExists(handle))
            return;

        var body = _simulation.Bodies.GetBodyReference(handle);
        body.Velocity.Linear = velocity;
        body.Awake = true;
    }

    public void TeleportBody(RigidbodyComponent rigidbody, Vector3 position, Quaternion rotation, bool resetVelocity)
    {
        var transform = rigidbody.Entity.Transform;
        var normalizedRotation = NormalizeOrIdentity(rotation);
        transform.LocalPosition = position;
        transform.LocalRotation = normalizedRotation;

        if (_simulation == null)
        {
            if (resetVelocity)
                rigidbody.InitialLinearVelocity = Vector3.Zero;
            return;
        }

        if (!_bodyStates.TryGetValue(rigidbody.Entity.Id, out var state))
        {
            if (resetVelocity)
                rigidbody.InitialLinearVelocity = Vector3.Zero;
            return;
        }

        state.AuthoritativeLocalPosition = position;
        state.AuthoritativeLocalRotation = normalizedRotation;
        state.LastPublishedVisualLocalPosition = position;
        state.LastPublishedVisualLocalRotation = normalizedRotation;
        state.HasPublishedVisualPose = true;

        if (state.Rigidbody.MotionType == BodyMotionType.Static)
        {
            if (state.StaticHandle is StaticHandle staticHandle && _simulation.Statics.StaticExists(staticHandle))
            {
                var staticRef = _simulation.Statics.GetStaticReference(staticHandle);
                staticRef.Pose = BuildBodyPose(position, normalizedRotation, transform.LocalScale, state.Collider);
                staticRef.UpdateBounds();
            }

            return;
        }

        if (state.BodyHandle is not BodyHandle bodyHandle || !_simulation.Bodies.BodyExists(bodyHandle))
            return;

        var hasCharacterController = state.Entity.GetComponent<CharacterControllerComponent>() is { Enabled: true };
        var targetPose = BuildBodyPose(position, normalizedRotation, transform.LocalScale, state.Collider);
        if (hasCharacterController)
        {
            targetPose.Orientation = Quaternion.Identity;
            var offset = ComputeWorldCenterOffset(state.Collider, transform.LocalScale, targetPose.Orientation);
            targetPose.Position = position + offset;
        }

        var body = _simulation.Bodies.GetBodyReference(bodyHandle);
        body.Pose = targetPose;
        if (state.Rigidbody.MotionType == BodyMotionType.Kinematic)
        {
            // Teleports should not inject one-frame kinematic velocities into contacts.
            body.Velocity.Linear = Vector3.Zero;
            body.Velocity.Angular = Vector3.Zero;
            if (resetVelocity)
                rigidbody.InitialLinearVelocity = Vector3.Zero;
            state.PreviousKinematicTargetPose = targetPose;
            state.HasPreviousKinematicTargetPose = true;
        }
        else if (resetVelocity)
        {
            body.Velocity.Linear = Vector3.Zero;
            body.Velocity.Angular = Vector3.Zero;
            rigidbody.InitialLinearVelocity = Vector3.Zero;
        }

        body.Awake = true;
        SnapSimulationPoseHistory(state, targetPose, suppressInterpolation: true);
    }

    public void PublishInterpolatedVisualPoses()
    {
        if (_simulation == null)
            return;

        var projSettings = PhysicsProjectSettings.Current;
        projSettings.Normalize();

        var fixedDt = projSettings.FixedTimestep;
        if (!float.IsFinite(fixedDt) || fixedDt <= 0f)
            return;

        var alpha = Math.Clamp(_accumulator / fixedDt, 0f, 1f);

        foreach (var state in _bodyStates.Values)
        {
            if (state.Rigidbody.MotionType != BodyMotionType.Dynamic)
            {
                state.HasPublishedVisualPose = false;
                state.SuppressInterpolationForFrame = false;
                continue;
            }

            var transform = state.Entity.Transform;
            var currentTransformPosition = transform.LocalPosition;
            var currentTransformRotation = NormalizeOrIdentity(transform.LocalRotation);
            var hasCharacterController = state.Entity.GetComponent<CharacterControllerComponent>() is { Enabled: true };

            if (hasCharacterController)
                state.AuthoritativeLocalRotation = currentTransformRotation;

            var positionExternallyEdited = state.HasPublishedVisualPose
                ? !ApproximatelyEqual(currentTransformPosition, state.LastPublishedVisualLocalPosition)
                : !ApproximatelyEqual(currentTransformPosition, state.AuthoritativeLocalPosition);
            var rotationExternallyEdited = !hasCharacterController && (state.HasPublishedVisualPose
                ? !ApproximatelyEqual(currentTransformRotation, state.LastPublishedVisualLocalRotation)
                : !ApproximatelyEqual(currentTransformRotation, state.AuthoritativeLocalRotation));
            if (positionExternallyEdited || rotationExternallyEdited)
            {
                state.SuppressInterpolationForFrame = true;
                continue;
            }

            var authoritativePosition = state.AuthoritativeLocalPosition;
            var authoritativeRotation = state.AuthoritativeLocalRotation;

            if (!ShouldInterpolateBody(state, projSettings) || !state.HasSimulationPoseHistory)
            {
                transform.LocalPosition = authoritativePosition;
                transform.LocalRotation = authoritativeRotation;
                state.LastPublishedVisualLocalPosition = authoritativePosition;
                state.LastPublishedVisualLocalRotation = authoritativeRotation;
                state.HasPublishedVisualPose = true;
                state.SuppressInterpolationForFrame = false;
                continue;
            }

            var previousPose = state.PreviousSimulationPose;
            var currentPose = state.CurrentSimulationPose;
            var useCurrentPose = state.SuppressInterpolationForFrame;
            var visualPose = useCurrentPose
                ? currentPose
                : InterpolatePose(previousPose, currentPose, alpha);

            var visualPosition = visualPose.Position - ComputeWorldCenterOffset(state.Collider, transform.LocalScale, visualPose.Orientation);
            var visualRotation = hasCharacterController
                ? authoritativeRotation
                : NormalizeOrIdentity(visualPose.Orientation);

            transform.LocalPosition = visualPosition;
            transform.LocalRotation = visualRotation;
            state.LastPublishedVisualLocalPosition = visualPosition;
            state.LastPublishedVisualLocalRotation = visualRotation;
            state.HasPublishedVisualPose = true;
            state.SuppressInterpolationForFrame = false;
        }
    }

    private void ReconcileParticipants()
    {
        if (_simulation == null)
            return;

        var seenEntityIds = new HashSet<Guid>();
        var rigidbodies = _scene.GetComponents<RigidbodyComponent>();

        foreach (var rigidbody in rigidbodies)
        {
            if (!rigidbody.Enabled || !rigidbody.Entity.Active)
                continue;
            if (rigidbody.Entity.Scene != _scene)
                continue;

            var entity = rigidbody.Entity;
            if (!seenEntityIds.Add(entity.Id))
            {
                WarnOnce(_warnedMultipleRigidbodies, entity.Id, $"Entity '{entity.Name}' has multiple RigidbodyComponent instances. Only the first one is used.");
                continue;
            }

            if (entity.Transform.Parent != null)
            {
                RemoveStateIfPresent(entity.Id);
                WarnOnce(_warnedParented, entity.Id, $"Rigidbody on '{entity.Name}' is ignored because parented rigidbodies are not supported.");
                continue;
            }
            _warnedParented.Remove(entity.Id);

            if (!TryGetPrimaryCollider(entity, out var collider, out var hasMultipleColliders))
            {
                RemoveStateIfPresent(entity.Id);
                WarnOnce(_warnedNoCollider, entity.Id, $"Rigidbody on '{entity.Name}' has no enabled collider and will be ignored.");
                continue;
            }
            _warnedNoCollider.Remove(entity.Id);

            if (hasMultipleColliders)
            {
                WarnOnce(_warnedMultipleColliders, entity.Id, $"Entity '{entity.Name}' has multiple collider components. Only the first enabled collider is used.");
            }
            else
            {
                _warnedMultipleColliders.Remove(entity.Id);
            }

            var configHash = ComputeConfigurationHash(entity, rigidbody, collider);
            if (_bodyStates.TryGetValue(entity.Id, out var existing))
            {
                if (existing.ConfigurationHash == configHash &&
                    ReferenceEquals(existing.Rigidbody, rigidbody) &&
                    ReferenceEquals(existing.Collider, collider))
                {
                    continue;
                }

                // Capture velocity before destroying body to preserve momentum through rebuild
                Vector3 preservedLinearVelocity = Vector3.Zero;
                Vector3 preservedAngularVelocity = Vector3.Zero;
                bool shouldPreserveVelocity = false;

                if (existing.BodyHandle is BodyHandle bodyHandle &&
                    _simulation.Bodies.BodyExists(bodyHandle))
                {
                    var body = _simulation.Bodies.GetBodyReference(bodyHandle);
                    preservedLinearVelocity = body.Velocity.Linear;
                    preservedAngularVelocity = body.Velocity.Angular;
                    shouldPreserveVelocity = true;
                }

                RemoveBodyState(existing);
                _bodyStates.Remove(entity.Id);

                // Create new body and restore velocity to maintain physics continuity
                var rebuiltState = CreateBodyState(entity, rigidbody, collider, configHash);
                if (rebuiltState != null)
                {
                    _bodyStates[entity.Id] = rebuiltState;

                    if (shouldPreserveVelocity &&
                        rebuiltState.BodyHandle is BodyHandle newBodyHandle &&
                        _simulation.Bodies.BodyExists(newBodyHandle))
                    {
                        var newBody = _simulation.Bodies.GetBodyReference(newBodyHandle);
                        newBody.Velocity.Linear = preservedLinearVelocity;
                        newBody.Velocity.Angular = preservedAngularVelocity;
                        newBody.Awake = true;
                    }
                }

                continue;
            }

            var newState = CreateBodyState(entity, rigidbody, collider, configHash);
            if (newState != null)
                _bodyStates[entity.Id] = newState;
        }

        var staleIds = _bodyStates.Keys.Where(id => !seenEntityIds.Contains(id)).ToList();
        foreach (var staleId in staleIds)
        {
            if (_bodyStates.TryGetValue(staleId, out var state))
                RemoveBodyState(state);
            _bodyStates.Remove(staleId);
        }

        ReconcileCharacterControllers();
    }

    private void ReconcileCharacterControllers()
    {
        if (_simulation == null || _characterControllers == null)
            return;

        var seenEntityIds = new HashSet<Guid>();
        var controllers = _scene.GetComponents<CharacterControllerComponent>();

        foreach (var controller in controllers)
        {
            if (!controller.Enabled || !controller.Entity.Active)
                continue;
            if (controller.Entity.Scene != _scene)
                continue;

            var entity = controller.Entity;
            if (!seenEntityIds.Add(entity.Id))
                continue;

            if (entity.Transform.Parent != null)
            {
                RemoveCharacterStateIfPresent(entity.Id);
                WarnOnce(_warnedCharacterParented, entity.Id, $"Character controller on '{entity.Name}' is ignored because parented rigidbodies are not supported.");
                continue;
            }
            _warnedCharacterParented.Remove(entity.Id);

            var rigidbody = entity.GetComponent<RigidbodyComponent>();
            if (rigidbody == null || !rigidbody.Enabled)
            {
                RemoveCharacterStateIfPresent(entity.Id);
                WarnOnce(_warnedCharacterMissingRigidbody, entity.Id, $"Character controller on '{entity.Name}' requires an enabled RigidbodyComponent.");
                continue;
            }
            _warnedCharacterMissingRigidbody.Remove(entity.Id);

            var capsule = entity.GetComponent<CapsuleColliderComponent>();
            if (capsule == null || !capsule.Enabled)
            {
                RemoveCharacterStateIfPresent(entity.Id);
                WarnOnce(_warnedCharacterMissingCapsule, entity.Id, $"Character controller on '{entity.Name}' requires an enabled CapsuleColliderComponent.");
                continue;
            }
            _warnedCharacterMissingCapsule.Remove(entity.Id);

            if (rigidbody.MotionType != BodyMotionType.Dynamic)
            {
                RemoveCharacterStateIfPresent(entity.Id);
                WarnOnce(_warnedCharacterWrongMotionType, entity.Id, $"Character controller on '{entity.Name}' requires Rigidbody MotionType = Dynamic.");
                continue;
            }
            _warnedCharacterWrongMotionType.Remove(entity.Id);

            if (!_bodyStates.TryGetValue(entity.Id, out var bodyState) ||
                bodyState.BodyHandle is not BodyHandle bodyHandle ||
                !_simulation.Bodies.BodyExists(bodyHandle))
            {
                RemoveCharacterStateIfPresent(entity.Id);
                continue;
            }

            if (bodyState.Collider is not CapsuleColliderComponent primaryCapsule || !ReferenceEquals(primaryCapsule, capsule))
            {
                RemoveCharacterStateIfPresent(entity.Id);
                WarnOnce(_warnedCharacterNonCapsuleBody, entity.Id, $"Character controller on '{entity.Name}' requires the primary enabled collider to be the capsule.");
                continue;
            }
            _warnedCharacterNonCapsuleBody.Remove(entity.Id);

            EnsureCharacterState(entity, rigidbody, primaryCapsule, controller, bodyHandle);
        }

        var staleIds = _characterStates.Keys.Where(id => !seenEntityIds.Contains(id)).ToList();
        foreach (var staleId in staleIds)
            RemoveCharacterStateIfPresent(staleId);
    }

    private void EnsureCharacterState(
        Entity entity,
        RigidbodyComponent rigidbody,
        CapsuleColliderComponent capsule,
        CharacterControllerComponent controller,
        BodyHandle bodyHandle)
    {
        if (_characterControllers == null)
            return;

        if (_characterStates.TryGetValue(entity.Id, out var existing))
        {
            if (existing.BodyHandle.Value != bodyHandle.Value)
            {
                RemoveCharacterStateIfPresent(entity.Id);
            }
            else
            {
                existing.Entity = entity;
                existing.Rigidbody = rigidbody;
                existing.Capsule = capsule;
                existing.Controller = controller;
                return;
            }
        }

        ref var character = ref _characterControllers.AllocateCharacter(bodyHandle);
        character.BodyHandle = bodyHandle;
        character.LocalUp = Vector3.UnitY;
        character.ViewDirection = entity.Transform.Forward;
        character.TargetVelocity = Vector2.Zero;

        _characterStates[entity.Id] = new CharacterControllerRuntimeState
        {
            Entity = entity,
            Rigidbody = rigidbody,
            Capsule = capsule,
            Controller = controller,
            BodyHandle = bodyHandle,
            FrameInput = default
        };
    }

    private void RemoveCharacterStateIfPresent(Guid entityId)
    {
        if (!_characterStates.TryGetValue(entityId, out var state))
            return;

        if (_characterControllers != null)
            _characterControllers.RemoveCharacterByBodyHandle(state.BodyHandle);

        state.Controller.SetRuntimeState(false, Vector3.Zero);
        _characterStates.Remove(entityId);
    }

    private void RemoveCharacterStateByBodyHandle(BodyHandle bodyHandle)
    {
        if (_characterStates.Count == 0)
            return;

        Guid? matchedEntityId = null;
        foreach (var pair in _characterStates)
        {
            if (pair.Value.BodyHandle.Value != bodyHandle.Value)
                continue;

            matchedEntityId = pair.Key;
            break;
        }

        if (matchedEntityId.HasValue)
            RemoveCharacterStateIfPresent(matchedEntityId.Value);
    }

    private void ApplyCharacterGoalsForStep(float stepDt, bool allowJump)
    {
        if (_simulation == null || _characterControllers == null || _characterStates.Count == 0)
            return;

        _characterBridge.ApplyGoalsForStep(_simulation, _characterControllers, _characterStates.Values, stepDt, allowJump);
    }

    private void SyncCharacterRuntimeState()
    {
        if (_simulation == null || _characterControllers == null || _characterStates.Count == 0)
            return;

        _characterBridge.SyncRuntimeState(_simulation, _characterControllers, _characterStates.Values);
    }

    private PhysicsBodyState? CreateBodyState(Entity entity, RigidbodyComponent rigidbody, ColliderComponent collider, int configurationHash)
    {
        if (_simulation == null)
            return null;

        var transform = entity.Transform;
        var authoritativePosition = transform.LocalPosition;
        var authoritativeRotation = NormalizeOrIdentity(transform.LocalRotation);
        var mass = MathF.Max(0.0001f, rigidbody.Mass);
        var shapeResult = CreateShape(collider, transform.LocalScale, mass);
        var pose = BuildBodyPose(authoritativePosition, authoritativeRotation, transform.LocalScale, collider);
        var continuity = rigidbody.ContinuousDetection
            ? ContinuousDetection.Continuous()
            : ContinuousDetection.Discrete;
        var collidable = new CollidableDescription(shapeResult.ShapeIndex, 0.1f, continuity);
        var material = new PhysicsMaterial(collider.Friction, collider.Restitution);
        var hasCharacterController = entity.GetComponent<CharacterControllerComponent>() is { Enabled: true };

        BodyHandle? bodyHandle = null;
        StaticHandle? staticHandle = null;

        switch (rigidbody.MotionType)
        {
            case BodyMotionType.Dynamic:
            {
                var dynamicInertia = shapeResult.DynamicInertia;
                if (hasCharacterController)
                {
                    dynamicInertia.InverseInertiaTensor = default;
                    pose.Orientation = Quaternion.Identity;
                }

                var velocity = new BodyVelocity { Linear = rigidbody.InitialLinearVelocity };
                var activity = new BodyActivityDescription(0.01f);
                var description = BodyDescription.CreateDynamic(pose, velocity, dynamicInertia, collidable, activity);
                bodyHandle = _simulation.Bodies.Add(description);
                _materialTable.Set(bodyHandle.Value, material);
                break;
            }
            case BodyMotionType.Kinematic:
            {
                var velocity = new BodyVelocity { Linear = rigidbody.InitialLinearVelocity };
                var activity = new BodyActivityDescription(0.01f);
                var description = BodyDescription.CreateKinematic(pose, velocity, collidable, activity);
                bodyHandle = _simulation.Bodies.Add(description);
                _materialTable.Set(bodyHandle.Value, material);
                break;
            }
            case BodyMotionType.Static:
            {
                var description = new StaticDescription(pose, shapeResult.ShapeIndex, continuity);
                staticHandle = _simulation.Statics.Add(description);
                _materialTable.Set(staticHandle.Value, material);
                break;
            }
            default:
                return null;
        }

        return new PhysicsBodyState
        {
            Entity = entity,
            Rigidbody = rigidbody,
            Collider = collider,
            BodyHandle = bodyHandle,
            StaticHandle = staticHandle,
            ConfigurationHash = configurationHash,
            LockedPosition = authoritativePosition,
            LockReferenceOrientation = NormalizeOrIdentity(pose.Orientation),
            RotationLockMask = GetRotationLockMask(rigidbody),
            AuthoritativeLocalPosition = authoritativePosition,
            AuthoritativeLocalRotation = authoritativeRotation,
            LastPublishedVisualLocalPosition = authoritativePosition,
            LastPublishedVisualLocalRotation = authoritativeRotation,
            HasPublishedVisualPose = true,
            PreviousSimulationPose = pose,
            CurrentSimulationPose = pose,
            HasSimulationPoseHistory = true,
            PreviousKinematicTargetPose = pose,
            HasPreviousKinematicTargetPose = rigidbody.MotionType == BodyMotionType.Kinematic
        };
    }

    private void RemoveStateIfPresent(Guid entityId)
    {
        if (_bodyStates.TryGetValue(entityId, out var state))
        {
            RemoveBodyState(state);
            _bodyStates.Remove(entityId);
        }
    }

    private void RemoveBodyState(PhysicsBodyState state)
    {
        if (_simulation == null)
            return;

        if (state.BodyHandle is BodyHandle bodyHandle)
        {
            RemoveCharacterStateByBodyHandle(bodyHandle);
            _materialTable.Remove(bodyHandle);
            if (_simulation.Bodies.BodyExists(bodyHandle))
                _simulation.Bodies.Remove(bodyHandle);
        }

        if (state.StaticHandle is StaticHandle staticHandle)
        {
            _materialTable.Remove(staticHandle);
            if (_simulation.Statics.StaticExists(staticHandle))
                _simulation.Statics.Remove(staticHandle);
        }
    }

    private void PushSceneTransformsToPhysics(float stepDt)
    {
        if (_simulation == null)
            return;

        foreach (var state in _bodyStates.Values)
        {
            var transform = state.Entity.Transform;
            var currentTransformPosition = transform.LocalPosition;
            var currentTransformRotation = NormalizeOrIdentity(transform.LocalRotation);
            var hasCharacterController = state.Entity.GetComponent<CharacterControllerComponent>() is { Enabled: true };

            if (state.Rigidbody.MotionType == BodyMotionType.Static)
            {
                if (state.StaticHandle is not StaticHandle staticHandle || !_simulation.Statics.StaticExists(staticHandle))
                    continue;

                state.AuthoritativeLocalPosition = currentTransformPosition;
                state.AuthoritativeLocalRotation = currentTransformRotation;
                var targetPose = BuildBodyPose(state.AuthoritativeLocalPosition, state.AuthoritativeLocalRotation, transform.LocalScale, state.Collider);
                var staticRef = _simulation.Statics.GetStaticReference(staticHandle);
                staticRef.Pose = targetPose;
                staticRef.UpdateBounds();
                continue;
            }

            if (state.BodyHandle is not BodyHandle bodyHandle || !_simulation.Bodies.BodyExists(bodyHandle))
                continue;

            var body = _simulation.Bodies.GetBodyReference(bodyHandle);
            if (state.Rigidbody.MotionType == BodyMotionType.Kinematic)
            {
                state.AuthoritativeLocalPosition = currentTransformPosition;
                state.AuthoritativeLocalRotation = currentTransformRotation;
                var targetPose = BuildBodyPose(state.AuthoritativeLocalPosition, state.AuthoritativeLocalRotation, transform.LocalScale, state.Collider);
                var previousTargetPose = state.HasPreviousKinematicTargetPose
                    ? state.PreviousKinematicTargetPose
                    : body.Pose;

                var previousOrientation = NormalizeOrIdentity(previousTargetPose.Orientation);
                var targetOrientation = EnsureSameHemisphere(previousOrientation, targetPose.Orientation);
                targetPose = new RigidPose(targetPose.Position, targetOrientation);

                var linearVelocity = (targetPose.Position - previousTargetPose.Position) / stepDt;
                var angularVelocity = ComputeAngularVelocity(previousOrientation, targetOrientation, stepDt);
                var linearSpeed = linearVelocity.Length();
                var angularStep = ComputeAngularStepRadians(previousOrientation, targetOrientation);

                var maxAngularStepRadians = MaxKinematicAngularStepDegrees * (MathF.PI / 180f);
                var maxDiscontinuityLinearSpeed = MaxKinematicLinearSpeed * KinematicDiscontinuityLinearSpeedMultiplier;
                var hasDiscontinuity = linearSpeed > maxDiscontinuityLinearSpeed ||
                                       angularStep > maxAngularStepRadians;

                if (hasDiscontinuity)
                {
                    body.Velocity.Linear = Vector3.Zero;
                    body.Velocity.Angular = Vector3.Zero;
                    WarnOnce(
                        _warnedKinematicDiscontinuity,
                        state.Entity.Id,
                        $"Kinematic body '{state.Entity.Name}' had a discontinuous target step. Velocities were suppressed for stability.");
                }
                else
                {
                    body.Velocity.Linear = ClampMagnitude(linearVelocity, MaxKinematicLinearSpeed);
                    body.Velocity.Angular = ClampMagnitude(angularVelocity, MaxKinematicAngularSpeed);
                }

                body.Pose = targetPose;
                body.Awake = true;
                state.PreviousKinematicTargetPose = targetPose;
                state.HasPreviousKinematicTargetPose = true;
                continue;
            }

            var positionExternallyEdited = state.HasPublishedVisualPose
                ? !ApproximatelyEqual(currentTransformPosition, state.LastPublishedVisualLocalPosition)
                : !ApproximatelyEqual(currentTransformPosition, state.AuthoritativeLocalPosition);
            var rotationExternallyEdited = !hasCharacterController && (state.HasPublishedVisualPose
                ? !ApproximatelyEqual(currentTransformRotation, state.LastPublishedVisualLocalRotation)
                : !ApproximatelyEqual(currentTransformRotation, state.AuthoritativeLocalRotation));

            if (hasCharacterController)
            {
                state.AuthoritativeLocalRotation = currentTransformRotation;

                if (positionExternallyEdited)
                {
                    var currentPose = body.Pose;
                    currentPose.Orientation = Quaternion.Identity;
                    var offset = ComputeWorldCenterOffset(state.Collider, transform.LocalScale, currentPose.Orientation);
                    currentPose.Position = currentTransformPosition + offset;
                    body.Pose = currentPose;
                    body.Awake = true;

                    state.AuthoritativeLocalPosition = currentTransformPosition;
                    state.LastPublishedVisualLocalPosition = currentTransformPosition;
                    state.LastPublishedVisualLocalRotation = currentTransformRotation;
                    state.HasPublishedVisualPose = true;
                    SnapSimulationPoseHistory(state, currentPose, suppressInterpolation: true);
                }

                continue;
            }

            if (positionExternallyEdited || rotationExternallyEdited)
            {
                state.AuthoritativeLocalPosition = currentTransformPosition;
                state.AuthoritativeLocalRotation = currentTransformRotation;
                var targetPose = BuildBodyPose(state.AuthoritativeLocalPosition, state.AuthoritativeLocalRotation, transform.LocalScale, state.Collider);
                body.Pose = targetPose;
                body.Awake = true;
                state.LastPublishedVisualLocalPosition = currentTransformPosition;
                state.LastPublishedVisualLocalRotation = currentTransformRotation;
                state.HasPublishedVisualPose = true;
                SnapSimulationPoseHistory(state, targetPose, suppressInterpolation: true);
            }
        }
    }

    private void ApplyPendingForces(float stepDt)
    {
        if (_simulation == null)
            return;

        foreach (var state in _bodyStates.Values)
        {
            if (state.Rigidbody.MotionType != BodyMotionType.Dynamic)
                continue;
            if (state.BodyHandle is not BodyHandle bodyHandle || !_simulation.Bodies.BodyExists(bodyHandle))
                continue;

            state.Rigidbody.ConsumePendingForces(out var force, out var impulse);
            if (force == Vector3.Zero && impulse == Vector3.Zero)
                continue;

            var combinedImpulse = impulse + force * stepDt;
            if (combinedImpulse == Vector3.Zero)
                continue;

            var body = _simulation.Bodies.GetBodyReference(bodyHandle);
            body.ApplyLinearImpulse(combinedImpulse);
            body.Awake = true;
        }
    }

    private void ApplyPostStepBodyModifiers(float stepDt)
    {
        if (_simulation == null)
            return;

        foreach (var state in _bodyStates.Values)
        {
            if (state.Rigidbody.MotionType != BodyMotionType.Dynamic)
                continue;
            if (state.BodyHandle is not BodyHandle bodyHandle || !_simulation.Bodies.BodyExists(bodyHandle))
                continue;

            var body = _simulation.Bodies.GetBodyReference(bodyHandle);
            var linear = body.Velocity.Linear;
            var angular = body.Velocity.Angular;

            var linearDampingFactor = MathF.Pow(Math.Clamp(1f - state.Rigidbody.LinearDamping, 0f, 1f), stepDt);
            var angularDampingFactor = MathF.Pow(Math.Clamp(1f - state.Rigidbody.AngularDamping, 0f, 1f), stepDt);
            linear *= linearDampingFactor;
            angular *= angularDampingFactor;

            var pose = body.Pose;
            var position = pose.Position;
            var lockedPosition = state.LockedPosition;

            if (state.Rigidbody.LockPositionX) { position.X = lockedPosition.X; linear.X = 0f; } else { lockedPosition.X = position.X; }
            if (state.Rigidbody.LockPositionY) { position.Y = lockedPosition.Y; linear.Y = 0f; } else { lockedPosition.Y = position.Y; }
            if (state.Rigidbody.LockPositionZ) { position.Z = lockedPosition.Z; linear.Z = 0f; } else { lockedPosition.Z = position.Z; }
            state.LockedPosition = lockedPosition;

            var rotationLockMask = GetRotationLockMask(state.Rigidbody);
            if (rotationLockMask == 0)
            {
                state.LockReferenceOrientation = NormalizeOrIdentity(pose.Orientation);
                state.RotationLockMask = 0;
            }
            else
            {
                if (state.RotationLockMask != rotationLockMask)
                    state.LockReferenceOrientation = NormalizeOrIdentity(pose.Orientation);

                ApplyWorldRotationLocksStrict(ref pose, ref angular, state, rotationLockMask);
                state.RotationLockMask = rotationLockMask;
            }

            pose.Position = position;
            pose.Orientation = NormalizeOrIdentity(pose.Orientation);
            body.Pose = pose;
            body.Velocity.Linear = linear;
            body.Velocity.Angular = angular;
        }
    }

    private void SyncDynamicTransformsFromPhysics()
    {
        if (_simulation == null)
            return;

        foreach (var state in _bodyStates.Values)
        {
            if (state.Rigidbody.MotionType != BodyMotionType.Dynamic)
                continue;
            if (state.BodyHandle is not BodyHandle bodyHandle || !_simulation.Bodies.BodyExists(bodyHandle))
                continue;

            var body = _simulation.Bodies.GetBodyReference(bodyHandle);
            var pose = body.Pose;
            var transform = state.Entity.Transform;
            var hasCharacterController = state.Entity.GetComponent<CharacterControllerComponent>() is { Enabled: true };

            var offset = ComputeWorldCenterOffset(state.Collider, transform.LocalScale, pose.Orientation);
            var authoritativePosition = pose.Position - offset;
            var authoritativeRotation = state.AuthoritativeLocalRotation;
            if (!hasCharacterController)
                authoritativeRotation = NormalizeOrIdentity(pose.Orientation);

            state.AuthoritativeLocalPosition = authoritativePosition;
            state.AuthoritativeLocalRotation = authoritativeRotation;
            CaptureSimulationPoseAfterStep(state, pose);
        }
    }

    private ShapeCreationResult CreateShape(ColliderComponent collider, Vector3 localScale, float mass)
    {
        if (_simulation == null)
            throw new InvalidOperationException("Physics simulation is not initialized.");

        var absScale = new Vector3(MathF.Abs(localScale.X), MathF.Abs(localScale.Y), MathF.Abs(localScale.Z));
        absScale.X = MathF.Max(absScale.X, 0.0001f);
        absScale.Y = MathF.Max(absScale.Y, 0.0001f);
        absScale.Z = MathF.Max(absScale.Z, 0.0001f);

        switch (collider)
        {
            case BoxColliderComponent box:
            {
                var dimensions = box.Size * absScale;
                dimensions.X = MathF.Max(0.001f, dimensions.X);
                dimensions.Y = MathF.Max(0.001f, dimensions.Y);
                dimensions.Z = MathF.Max(0.001f, dimensions.Z);
                var shape = new Box(dimensions.X, dimensions.Y, dimensions.Z);
                var key = ShapeCacheKey.Box(dimensions.X, dimensions.Y, dimensions.Z);
                if (!_shapeCache.TryGetValue(key, out var shapeIndex))
                {
                    shapeIndex = _simulation.Shapes.Add(shape);
                    _shapeCache[key] = shapeIndex;
                }
                return new ShapeCreationResult
                {
                    ShapeIndex = shapeIndex,
                    DynamicInertia = shape.ComputeInertia(mass)
                };
            }
            case SphereColliderComponent sphere:
            {
                var radiusScale = MathF.Max(absScale.X, MathF.Max(absScale.Y, absScale.Z));
                var radius = MathF.Max(0.001f, sphere.Radius * radiusScale);
                var shape = new Sphere(radius);
                var key = ShapeCacheKey.Sphere(radius);
                if (!_shapeCache.TryGetValue(key, out var shapeIndex))
                {
                    shapeIndex = _simulation.Shapes.Add(shape);
                    _shapeCache[key] = shapeIndex;
                }
                return new ShapeCreationResult
                {
                    ShapeIndex = shapeIndex,
                    DynamicInertia = shape.ComputeInertia(mass)
                };
            }
            case CapsuleColliderComponent capsule:
            {
                var radiusScale = MathF.Max(absScale.X, absScale.Z);
                var radius = MathF.Max(0.001f, capsule.Radius * radiusScale);
                var length = MathF.Max(0.001f, capsule.Length * absScale.Y);
                var shape = new Capsule(radius, length);
                var key = ShapeCacheKey.Capsule(radius, length);
                if (!_shapeCache.TryGetValue(key, out var shapeIndex))
                {
                    shapeIndex = _simulation.Shapes.Add(shape);
                    _shapeCache[key] = shapeIndex;
                }
                return new ShapeCreationResult
                {
                    ShapeIndex = shapeIndex,
                    DynamicInertia = shape.ComputeInertia(mass)
                };
            }
            default:
                throw new NotSupportedException($"Collider type '{collider.GetType().Name}' is not supported.");
        }
    }

    private static RigidPose BuildBodyPose(TransformComponent transform, ColliderComponent collider)
    {
        return BuildBodyPose(transform.LocalPosition, transform.LocalRotation, transform.LocalScale, collider);
    }

    private static RigidPose BuildBodyPose(Vector3 position, Quaternion rotation, Vector3 localScale, ColliderComponent collider)
    {
        var orientation = NormalizeOrIdentity(rotation);
        var offset = ComputeWorldCenterOffset(collider, localScale, orientation);
        return new RigidPose(position + offset, orientation);
    }

    private static Vector3 ComputeWorldCenterOffset(ColliderComponent collider, Vector3 localScale, Quaternion orientation)
    {
        var scaledCenter = new Vector3(
            collider.Center.X * localScale.X,
            collider.Center.Y * localScale.Y,
            collider.Center.Z * localScale.Z);
        return Vector3.Transform(scaledCenter, orientation);
    }

    private static int ComputeConfigurationHash(Entity entity, RigidbodyComponent rigidbody, ColliderComponent collider)
    {
        var hash = new HashCode();
        hash.Add(rigidbody.SettingsVersion);
        hash.Add(collider.SettingsVersion);
        hash.Add((int)rigidbody.MotionType);
        hash.Add(collider.GetType());
        hash.Add(entity.Transform.LocalScale.X);
        hash.Add(entity.Transform.LocalScale.Y);
        hash.Add(entity.Transform.LocalScale.Z);
        hash.Add(entity.GetComponent<CharacterControllerComponent>() is { Enabled: true });
        return hash.ToHashCode();
    }

    private static byte GetRotationLockMask(RigidbodyComponent rigidbody)
    {
        byte mask = 0;
        if (rigidbody.LockRotationX)
            mask |= RotationLockXMask;
        if (rigidbody.LockRotationY)
            mask |= RotationLockYMask;
        if (rigidbody.LockRotationZ)
            mask |= RotationLockZMask;
        return mask;
    }

    private static void ApplyWorldRotationLocksStrict(ref RigidPose pose, ref Vector3 angularVelocity, PhysicsBodyState state, byte lockMask)
    {
        if ((lockMask & RotationLockXMask) != 0)
            angularVelocity.X = 0f;
        if ((lockMask & RotationLockYMask) != 0)
            angularVelocity.Y = 0f;
        if ((lockMask & RotationLockZMask) != 0)
            angularVelocity.Z = 0f;

        pose.Orientation = FilterRotationDeltaByMask(state.LockReferenceOrientation, pose.Orientation, lockMask);
    }

    private static Quaternion FilterRotationDeltaByMask(Quaternion reference, Quaternion current, byte lockMask)
    {
        var normalizedReference = NormalizeOrIdentity(reference);
        var normalizedCurrent = NormalizeOrIdentity(current);

        var delta = NormalizeOrIdentity(normalizedCurrent * Quaternion.Conjugate(normalizedReference));
        var rotationVector = QuaternionToRotationVector(delta);
        if ((lockMask & RotationLockXMask) != 0)
            rotationVector.X = 0f;
        if ((lockMask & RotationLockYMask) != 0)
            rotationVector.Y = 0f;
        if ((lockMask & RotationLockZMask) != 0)
            rotationVector.Z = 0f;

        var filteredDelta = RotationVectorToQuaternion(rotationVector);
        return NormalizeOrIdentity(filteredDelta * normalizedReference);
    }

    private static Vector3 QuaternionToRotationVector(Quaternion rotation)
    {
        var normalized = NormalizeOrIdentity(rotation);
        if (normalized.W < 0f)
            normalized = new Quaternion(-normalized.X, -normalized.Y, -normalized.Z, -normalized.W);

        var w = Math.Clamp(normalized.W, -1f, 1f);
        var angle = 2f * MathF.Acos(w);
        if (angle < 1e-6f)
            return Vector3.Zero;

        var sinHalf = MathF.Sqrt(MathF.Max(0f, 1f - w * w));
        if (sinHalf < 1e-6f)
            return Vector3.Zero;

        var axis = new Vector3(normalized.X, normalized.Y, normalized.Z) / sinHalf;
        return axis * angle;
    }

    private static Quaternion RotationVectorToQuaternion(Vector3 rotationVector)
    {
        var angle = rotationVector.Length();
        if (angle < 1e-6f)
            return Quaternion.Identity;

        var axis = rotationVector / angle;
        var halfAngle = angle * 0.5f;
        var sinHalf = MathF.Sin(halfAngle);
        var cosHalf = MathF.Cos(halfAngle);
        var rotation = new Quaternion(axis.X * sinHalf, axis.Y * sinHalf, axis.Z * sinHalf, cosHalf);
        return NormalizeOrIdentity(rotation);
    }

    private static bool IsFinite(Quaternion rotation)
    {
        return float.IsFinite(rotation.X) &&
               float.IsFinite(rotation.Y) &&
               float.IsFinite(rotation.Z) &&
               float.IsFinite(rotation.W);
    }

    private static Quaternion NormalizeOrIdentity(Quaternion rotation)
    {
        if (!IsFinite(rotation))
            return Quaternion.Identity;

        var lengthSquared = rotation.LengthSquared();
        if (!float.IsFinite(lengthSquared) || lengthSquared <= 1e-12f)
            return Quaternion.Identity;

        return Quaternion.Normalize(rotation);
    }

    private static bool TryGetPrimaryCollider(Entity entity, out ColliderComponent collider, out bool hasMultiple)
    {
        collider = null!;
        hasMultiple = false;

        ColliderComponent? first = null;
        int count = 0;

        foreach (var component in entity.Components)
        {
            if (component is not ColliderComponent candidate || !candidate.Enabled)
                continue;

            count++;
            first ??= candidate;
            if (count > 1)
                hasMultiple = true;
        }

        if (first == null)
            return false;

        collider = first;
        return true;
    }

    private static void WarnOnce(HashSet<Guid> warningSet, Guid entityId, string message)
    {
        if (!warningSet.Add(entityId))
            return;

        FrinkyLog.Warning(message);
    }

    private static bool ApproximatelyEqual(Vector3 a, Vector3 b, float epsilon = 1e-4f)
    {
        return MathF.Abs(a.X - b.X) <= epsilon &&
               MathF.Abs(a.Y - b.Y) <= epsilon &&
               MathF.Abs(a.Z - b.Z) <= epsilon;
    }

    private static bool ApproximatelyEqual(Quaternion a, Quaternion b, float epsilon = 1e-4f)
    {
        var dot = MathF.Abs(Quaternion.Dot(Quaternion.Normalize(a), Quaternion.Normalize(b)));
        return dot >= 1f - epsilon;
    }

    private static Vector3 ComputeAngularVelocity(Quaternion from, Quaternion to, float dt)
    {
        if (dt <= 0f)
            return Vector3.Zero;

        var normalizedFrom = NormalizeOrIdentity(from);
        var normalizedTo = EnsureSameHemisphere(normalizedFrom, to);
        var delta = NormalizeOrIdentity(normalizedTo * Quaternion.Conjugate(normalizedFrom));
        if (delta.W < 0f)
            delta = new Quaternion(-delta.X, -delta.Y, -delta.Z, -delta.W);

        var w = Math.Clamp(delta.W, -1f, 1f);
        var angle = 2f * MathF.Acos(w);
        if (angle < 1e-6f)
            return Vector3.Zero;

        var sinHalf = MathF.Sqrt(MathF.Max(0f, 1f - w * w));
        if (sinHalf < 1e-6f)
            return Vector3.Zero;

        var axis = new Vector3(delta.X, delta.Y, delta.Z) / sinHalf;
        return axis * (angle / dt);
    }

    private static float ComputeAngularStepRadians(Quaternion from, Quaternion to)
    {
        var normalizedFrom = NormalizeOrIdentity(from);
        var normalizedTo = EnsureSameHemisphere(normalizedFrom, to);
        var dot = Math.Clamp(Quaternion.Dot(normalizedFrom, normalizedTo), -1f, 1f);
        var angle = 2f * MathF.Acos(dot);
        if (!float.IsFinite(angle))
            return 0f;
        return angle;
    }

    private static Quaternion EnsureSameHemisphere(Quaternion reference, Quaternion candidate)
    {
        var normalizedReference = NormalizeOrIdentity(reference);
        var normalizedCandidate = NormalizeOrIdentity(candidate);
        if (Quaternion.Dot(normalizedReference, normalizedCandidate) < 0f)
        {
            normalizedCandidate = new Quaternion(
                -normalizedCandidate.X,
                -normalizedCandidate.Y,
                -normalizedCandidate.Z,
                -normalizedCandidate.W);
        }

        return normalizedCandidate;
    }

    private static Vector3 ClampMagnitude(Vector3 vector, float maxLength)
    {
        if (maxLength <= 0f)
            return Vector3.Zero;

        var length = vector.Length();
        if (!float.IsFinite(length) || length <= 1e-6f)
            return Vector3.Zero;
        if (length <= maxLength)
            return vector;

        return vector * (maxLength / length);
    }

    private static bool ShouldInterpolateBody(PhysicsBodyState state, PhysicsProjectSettings settings)
    {
        return state.Rigidbody.InterpolationMode switch
        {
            RigidbodyInterpolationMode.None => false,
            RigidbodyInterpolationMode.Interpolate => true,
            _ => settings.InterpolationEnabled
        };
    }

    private static RigidPose InterpolatePose(RigidPose previous, RigidPose current, float alpha)
    {
        var clampedAlpha = Math.Clamp(alpha, 0f, 1f);
        var previousOrientation = NormalizeOrIdentity(previous.Orientation);
        var currentOrientation = NormalizeOrIdentity(current.Orientation);
        if (Quaternion.Dot(previousOrientation, currentOrientation) < 0f)
        {
            currentOrientation = new Quaternion(
                -currentOrientation.X,
                -currentOrientation.Y,
                -currentOrientation.Z,
                -currentOrientation.W);
        }

        return new RigidPose(
            Vector3.Lerp(previous.Position, current.Position, clampedAlpha),
            NormalizeOrIdentity(Quaternion.Slerp(previousOrientation, currentOrientation, clampedAlpha)));
    }

    private static void CaptureSimulationPoseAfterStep(PhysicsBodyState state, RigidPose pose)
    {
        if (!state.HasSimulationPoseHistory)
        {
            state.PreviousSimulationPose = pose;
            state.CurrentSimulationPose = pose;
            state.HasSimulationPoseHistory = true;
            return;
        }

        state.PreviousSimulationPose = state.CurrentSimulationPose;
        state.CurrentSimulationPose = pose;
    }

    private static void SnapSimulationPoseHistory(PhysicsBodyState state, RigidPose pose, bool suppressInterpolation)
    {
        state.PreviousSimulationPose = pose;
        state.CurrentSimulationPose = pose;
        state.HasSimulationPoseHistory = true;
        if (suppressInterpolation)
            state.SuppressInterpolationForFrame = true;
    }

    private readonly struct ShapeCreationResult
    {
        public required TypedIndex ShapeIndex { get; init; }
        public required BodyInertia DynamicInertia { get; init; }
    }

    public void Dispose()
    {
        if (_simulation == null)
            return;

        foreach (var state in _bodyStates.Values.ToList())
            RemoveBodyState(state);

        _bodyStates.Clear();
        _characterStates.Clear();
        _materialTable.Clear();
        _shapeCache.Clear();
        _warnedNoCollider.Clear();
        _warnedParented.Clear();
        _warnedMultipleColliders.Clear();
        _warnedMultipleRigidbodies.Clear();
        _warnedCharacterMissingRigidbody.Clear();
        _warnedCharacterMissingCapsule.Clear();
        _warnedCharacterWrongMotionType.Clear();
        _warnedCharacterParented.Clear();
        _warnedCharacterNonCapsuleBody.Clear();
        _warnedKinematicDiscontinuity.Clear();

        _characterControllers?.Dispose();
        _characterControllers = null;

        _simulation.Dispose();
        _simulation = null;
        _bufferPool.Clear();
        _accumulator = 0f;
    }
}
