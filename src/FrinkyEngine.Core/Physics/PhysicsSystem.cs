using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Constraints;
using BepuUtilities.Memory;
using FrinkyEngine.Core.Components;
using FrinkyEngine.Core.ECS;
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
        public Vector3 LockedEuler;
        public Vector3 LastTransformPosition;
        public Quaternion LastTransformRotation;
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
    private readonly HashSet<Guid> _warnedNoCollider = new();
    private readonly HashSet<Guid> _warnedParented = new();
    private readonly HashSet<Guid> _warnedMultipleColliders = new();
    private readonly HashSet<Guid> _warnedMultipleRigidbodies = new();

    private Simulation? _simulation;
    private float _accumulator;

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

        var settings = _scene.PhysicsSettings;
        var narrowPhaseCallbacks = new PhysicsNarrowPhaseCallbacks(
            new SpringSettings(settings.ContactSpringFrequency, settings.ContactDampingRatio),
            settings.MaximumRecoveryVelocity,
            settings.DefaultFriction,
            settings.DefaultRestitution,
            _materialTable);
        var poseCallbacks = new PhysicsPoseIntegratorCallbacks(settings.Gravity);
        var solveDescription = new SolveDescription(settings.SolverVelocityIterations, settings.SolverSubsteps);

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

        ReconcileParticipants();

        var settings = _scene.PhysicsSettings;
        settings.Normalize();
        var fixedDt = settings.FixedTimestep;
        var maxSubsteps = settings.MaxSubstepsPerFrame;

        _accumulator += dt;
        int steps = 0;

        while (_accumulator >= fixedDt && steps < maxSubsteps)
        {
            PushSceneTransformsToPhysics(fixedDt);
            ApplyPendingForces(fixedDt);
            _simulation.Timestep(fixedDt);
            ApplyPostStepBodyModifiers(fixedDt);
            SyncDynamicTransformsFromPhysics();

            _accumulator -= fixedDt;
            steps++;
        }

        if (steps >= maxSubsteps && _accumulator >= fixedDt)
            _accumulator = 0f;
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

        _warnedNoCollider.Remove(entity.Id);
        _warnedParented.Remove(entity.Id);
        _warnedMultipleColliders.Remove(entity.Id);
        _warnedMultipleRigidbodies.Remove(entity.Id);
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

                RemoveBodyState(existing);
                _bodyStates.Remove(entity.Id);
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
    }

    private PhysicsBodyState? CreateBodyState(Entity entity, RigidbodyComponent rigidbody, ColliderComponent collider, int configurationHash)
    {
        if (_simulation == null)
            return null;

        var transform = entity.Transform;
        var mass = MathF.Max(0.0001f, rigidbody.Mass);
        var shapeResult = CreateShape(collider, transform.LocalScale, mass);
        var pose = BuildBodyPose(transform, collider);
        var continuity = rigidbody.ContinuousDetection
            ? ContinuousDetection.Continuous()
            : ContinuousDetection.Discrete;
        var collidable = new CollidableDescription(shapeResult.ShapeIndex, 0.1f, continuity);
        var material = new PhysicsMaterial(collider.Friction, collider.Restitution);

        BodyHandle? bodyHandle = null;
        StaticHandle? staticHandle = null;

        switch (rigidbody.MotionType)
        {
            case BodyMotionType.Dynamic:
            {
                var velocity = new BodyVelocity { Linear = rigidbody.InitialLinearVelocity };
                var activity = new BodyActivityDescription(0.01f);
                var description = BodyDescription.CreateDynamic(pose, velocity, shapeResult.DynamicInertia, collidable, activity);
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
            LockedPosition = transform.LocalPosition,
            LockedEuler = FrinkyMath.QuaternionToEuler(transform.LocalRotation),
            LastTransformPosition = transform.LocalPosition,
            LastTransformRotation = transform.LocalRotation
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
            var targetPose = BuildBodyPose(transform, state.Collider);

            if (state.Rigidbody.MotionType == BodyMotionType.Static)
            {
                if (state.StaticHandle is not StaticHandle staticHandle || !_simulation.Statics.StaticExists(staticHandle))
                    continue;

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
                var currentPose = body.Pose;
                body.Velocity.Linear = (targetPose.Position - currentPose.Position) / stepDt;
                body.Velocity.Angular = ComputeAngularVelocity(currentPose.Orientation, targetPose.Orientation, stepDt);
                body.Pose = targetPose;
                body.Awake = true;
                continue;
            }

            // Allow explicit transform edits to teleport dynamics before stepping.
            if (!ApproximatelyEqual(transform.LocalPosition, state.LastTransformPosition) ||
                !ApproximatelyEqual(transform.LocalRotation, state.LastTransformRotation))
            {
                body.Pose = targetPose;
                body.Velocity.Linear = Vector3.Zero;
                body.Velocity.Angular = Vector3.Zero;
                body.Awake = true;
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

            var euler = FrinkyMath.QuaternionToEuler(pose.Orientation);
            var lockedEuler = state.LockedEuler;

            if (state.Rigidbody.LockRotationX) { euler.X = lockedEuler.X; angular.X = 0f; } else { lockedEuler.X = euler.X; }
            if (state.Rigidbody.LockRotationY) { euler.Y = lockedEuler.Y; angular.Y = 0f; } else { lockedEuler.Y = euler.Y; }
            if (state.Rigidbody.LockRotationZ) { euler.Z = lockedEuler.Z; angular.Z = 0f; } else { lockedEuler.Z = euler.Z; }
            state.LockedEuler = lockedEuler;

            pose.Position = position;
            pose.Orientation = Quaternion.Normalize(FrinkyMath.EulerToQuaternion(euler));
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

            var offset = ComputeWorldCenterOffset(state.Collider, transform.LocalScale, pose.Orientation);
            transform.LocalPosition = pose.Position - offset;
            transform.LocalRotation = Quaternion.Normalize(pose.Orientation);

            state.LastTransformPosition = transform.LocalPosition;
            state.LastTransformRotation = transform.LocalRotation;
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
        var orientation = Quaternion.Normalize(transform.LocalRotation);
        var offset = ComputeWorldCenterOffset(collider, transform.LocalScale, orientation);
        return new RigidPose(transform.LocalPosition + offset, orientation);
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
        return hash.ToHashCode();
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

        var delta = Quaternion.Normalize(to * Quaternion.Conjugate(from));
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
        _materialTable.Clear();
        _shapeCache.Clear();
        _warnedNoCollider.Clear();
        _warnedParented.Clear();
        _warnedMultipleColliders.Clear();
        _warnedMultipleRigidbodies.Clear();

        _simulation.Dispose();
        _simulation = null;
        _bufferPool.Clear();
        _accumulator = 0f;
    }
}
