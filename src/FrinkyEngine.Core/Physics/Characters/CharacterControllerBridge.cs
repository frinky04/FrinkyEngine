using System.Numerics;
using BepuPhysics;
using FrinkyEngine.Core.Components;
using FrinkyEngine.Core.ECS;

namespace FrinkyEngine.Core.Physics.Characters;

internal sealed class CharacterControllerRuntimeState
{
    public required Entity Entity;
    public required RigidbodyComponent Rigidbody;
    public required CapsuleColliderComponent Capsule;
    public required CharacterControllerComponent Controller;
    public BodyHandle BodyHandle;
    public CharacterControllerInputSnapshot FrameInput;
}

internal sealed class CharacterControllerBridge
{
    public void CaptureFrameInput(IReadOnlyCollection<CharacterControllerRuntimeState> states)
    {
        foreach (var state in states)
            state.FrameInput = state.Controller.CaptureInputSnapshot();
    }

    public void ConsumeFrameInput(IReadOnlyCollection<CharacterControllerRuntimeState> states)
    {
        foreach (var state in states)
            state.Controller.ConsumeInputSnapshot();
    }

    public void ApplyGoalsForStep(
        Simulation simulation,
        CharacterControllers characters,
        IReadOnlyCollection<CharacterControllerRuntimeState> states,
        float stepDt,
        bool allowJump)
    {
        foreach (var state in states)
        {
            if (!simulation.Bodies.BodyExists(state.BodyHandle))
                continue;

            ref var character = ref characters.GetCharacterByBodyHandle(state.BodyHandle);
            var body = simulation.Bodies.GetBodyReference(state.BodyHandle);

            ConfigureCharacter(ref character, state.Controller, state.Capsule, state.Entity.Transform.LocalScale);

            var up = Vector3.Transform(character.LocalUp, body.Pose.Orientation);
            if (!TryNormalize(ref up))
                up = Vector3.UnitY;

            var viewDirection = ResolveViewDirection(state.Controller, state.Entity, up);
            var desiredHorizontalVelocity = ComputeDesiredHorizontalVelocity(state.FrameInput, state.Controller, viewDirection, up);
            var targetVelocity = ToCharacterTargetVelocity(desiredHorizontalVelocity, viewDirection, up);
            var shouldJump = allowJump && state.FrameInput.JumpRequested;

            if (!body.Awake &&
                ((shouldJump && character.Supported) ||
                 targetVelocity != character.TargetVelocity ||
                 (targetVelocity != Vector2.Zero && !ApproximatelyEqual(viewDirection, character.ViewDirection))))
            {
                simulation.Awakener.AwakenBody(character.BodyHandle);
            }

            character.TargetVelocity = targetVelocity;
            character.ViewDirection = viewDirection;
            if (shouldJump)
                character.TryJump = true;

            if (!character.Supported && desiredHorizontalVelocity.LengthSquared() > 1e-12f)
            {
                ApplyAirControl(ref body, desiredHorizontalVelocity, character.MaximumHorizontalForce, state.Controller, stepDt);
            }

            state.Controller.SetRuntimeState(character.Supported, desiredHorizontalVelocity);
        }
    }

    public void SyncRuntimeState(
        Simulation simulation,
        CharacterControllers characters,
        IReadOnlyCollection<CharacterControllerRuntimeState> states)
    {
        foreach (var state in states)
        {
            if (!simulation.Bodies.BodyExists(state.BodyHandle))
                continue;

            ref var character = ref characters.GetCharacterByBodyHandle(state.BodyHandle);
            state.Controller.SetRuntimeState(character.Supported, state.Controller.LastComputedTargetVelocity);
        }
    }

    private static void ConfigureCharacter(
        ref CharacterController character,
        CharacterControllerComponent controller,
        CapsuleColliderComponent capsule,
        Vector3 localScale)
    {
        character.LocalUp = Vector3.UnitY;
        character.JumpVelocity = controller.JumpVelocity;
        character.MaximumHorizontalForce = controller.MaximumHorizontalForce;
        character.MaximumVerticalForce = controller.MaximumVerticalForce;
        character.CosMaximumSlope = MathF.Cos(controller.MaxSlopeDegrees * (MathF.PI / 180f));

        var absScale = new Vector3(
            MathF.Max(0.0001f, MathF.Abs(localScale.X)),
            MathF.Max(0.0001f, MathF.Abs(localScale.Y)),
            MathF.Max(0.0001f, MathF.Abs(localScale.Z)));
        var radiusScale = MathF.Max(absScale.X, absScale.Z);
        var scaledRadius = MathF.Max(0.001f, capsule.Radius * radiusScale);

        character.MinimumSupportDepth = scaledRadius * -0.01f;
        character.MinimumSupportContinuationDepth = -0.1f;
    }

    private static Vector3 ResolveViewDirection(
        CharacterControllerComponent controller,
        Entity entity,
        Vector3 up)
    {
        var raw = controller.UseEntityForwardAsViewDirection
            ? entity.Transform.Forward
            : controller.ViewDirectionOverride;

        if (!IsFinite(raw))
            raw = entity.Transform.Forward;

        var flattened = raw - up * Vector3.Dot(raw, up);
        if (!TryNormalize(ref flattened))
            flattened = BuildPerpendicular(up);

        return flattened;
    }

    private static Vector3 ComputeDesiredHorizontalVelocity(
        in CharacterControllerInputSnapshot input,
        CharacterControllerComponent controller,
        Vector3 viewDirection,
        Vector3 up)
    {
        var right = Vector3.Cross(viewDirection, up);
        if (!TryNormalize(ref right))
            right = BuildPerpendicular(up);

        var forward = Vector3.Cross(up, right);
        if (!TryNormalize(ref forward))
            forward = BuildPerpendicular(up);

        Vector3 desired;

        if (input.HasSlideVelocity)
        {
            desired = input.SlideVelocity;
        }
        else if (input.HasPlanarInput)
        {
            var planar = input.PlanarInput;
            var magnitude = planar.Length();
            if (magnitude > 1f)
                planar /= magnitude;

            desired = right * (planar.X * controller.MoveSpeed) +
                      forward * (planar.Y * controller.MoveSpeed);
        }
        else
        {
            var movement = input.WorldMovementInput;
            var movementMagnitude = movement.Length();
            if (movementMagnitude > 1f)
                movement /= movementMagnitude;

            desired = movement * controller.MoveSpeed;
        }

        desired -= up * Vector3.Dot(desired, up);
        return IsFinite(desired) ? desired : Vector3.Zero;
    }

    private static Vector2 ToCharacterTargetVelocity(Vector3 desiredHorizontalVelocity, Vector3 viewDirection, Vector3 up)
    {
        var right = Vector3.Cross(viewDirection, up);
        if (!TryNormalize(ref right))
            right = BuildPerpendicular(up);

        var forward = Vector3.Cross(up, right);
        if (!TryNormalize(ref forward))
            forward = BuildPerpendicular(up);

        return new Vector2(
            Vector3.Dot(desiredHorizontalVelocity, right),
            Vector3.Dot(desiredHorizontalVelocity, forward));
    }

    private static void ApplyAirControl(
        ref BodyReference body,
        Vector3 desiredHorizontalVelocity,
        float maximumHorizontalForce,
        CharacterControllerComponent controller,
        float stepDt)
    {
        if (controller.AirControlForceScale <= 0f || controller.AirControlSpeedScale <= 0f || stepDt <= 0f)
            return;

        var direction = desiredHorizontalVelocity;
        if (!TryNormalize(ref direction))
            return;

        var currentVelocity = Vector3.Dot(body.Velocity.Linear, direction);
        var airAccelerationDt = body.LocalInertia.InverseMass *
                                maximumHorizontalForce *
                                controller.AirControlForceScale *
                                stepDt;

        var maximumAirSpeed = desiredHorizontalVelocity.Length() * controller.AirControlSpeedScale;
        var targetVelocity = MathF.Min(currentVelocity + airAccelerationDt, maximumAirSpeed);
        var velocityChange = MathF.Max(0f, targetVelocity - currentVelocity);

        if (velocityChange <= 0f)
            return;

        body.Velocity.Linear += direction * velocityChange;
        body.Awake = true;
    }

    private static Vector3 BuildPerpendicular(Vector3 normal)
    {
        var basis = MathF.Abs(normal.Y) < 0.99f ? Vector3.UnitY : Vector3.UnitX;
        var perpendicular = Vector3.Cross(basis, normal);
        return TryNormalize(ref perpendicular) ? perpendicular : Vector3.UnitX;
    }

    private static bool TryNormalize(ref Vector3 value)
    {
        if (!IsFinite(value))
            return false;

        var lengthSquared = value.LengthSquared();
        if (lengthSquared <= 1e-12f)
            return false;

        value /= MathF.Sqrt(lengthSquared);
        return true;
    }

    private static bool IsFinite(Vector3 value)
    {
        return float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Z);
    }

    private static bool ApproximatelyEqual(Vector3 a, Vector3 b, float epsilon = 1e-4f)
    {
        return MathF.Abs(a.X - b.X) <= epsilon &&
               MathF.Abs(a.Y - b.Y) <= epsilon &&
               MathF.Abs(a.Z - b.Z) <= epsilon;
    }
}
