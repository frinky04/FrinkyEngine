# Physics

Physics is powered by [BepuPhysics 2](https://github.com/bepu/bepuphysics2).

## Rigidbodies

Add a `RigidbodyComponent` to give an entity physics behavior. Three motion types are available:

| Motion Type | Description |
|-------------|-------------|
| **Dynamic** | Fully simulated, affected by forces and collisions |
| **Kinematic** | Moves via transform, pushes dynamic bodies but is not affected by forces |
| **Static** | Immovable, used for terrain and walls |

### Kinematic Stability

- Contact-driving velocity is derived from consecutive kinematic target poses (continuity-aware), not from arbitrary pose snaps
- Kinematic linear and angular contact velocities are safety-clamped to avoid extreme one-frame impulses
- Large discontinuities (e.g. sudden large rotation jumps) are treated as teleport-style corrections for that step, with kinematic velocity suppressed

## Colliders

| Collider | Key Properties |
|----------|---------------|
| `BoxColliderComponent` | `Size` (1,1,1), `Center`, `IsTrigger` |
| `SphereColliderComponent` | `Radius` (0.5), `Center`, `IsTrigger` |
| `CapsuleColliderComponent` | `Radius` (0.5), `Length` (1.0), `Center`, `IsTrigger` |

All colliders support:
- **Center** offset from the entity origin
- **IsTrigger** mode for overlap detection without physical response

## Triggers

Set `IsTrigger = true` on any collider to make it a trigger volume. Trigger colliders detect overlaps but do not produce a physical response — objects pass through them.

### Trigger Callbacks

Override the trigger callbacks in your component to respond to overlaps:

```csharp
public class PickupZone : Component
{
    public override void OnTriggerEnter(Entity other)
    {
        // Called once when another entity first overlaps this trigger
        FrinkyLog.Info($"{other.Name} entered the zone!");
    }

    public override void OnTriggerStay(Entity other)
    {
        // Called each frame while the overlap persists
    }

    public override void OnTriggerExit(Entity other)
    {
        // Called once when the overlap ends
        FrinkyLog.Info($"{other.Name} left the zone!");
    }
}
```

Both entities in the overlap receive callbacks. At least one of the two colliders must have `IsTrigger` enabled. Trigger colliders still require a `RigidbodyComponent` on the same entity to participate in physics — the motion type can be Static, Kinematic, or Dynamic.

## Raycasting

Cast rays into the physics world to detect colliders:

```csharp
using FrinkyEngine.Core.Physics;

// Closest hit
if (Physics.Raycast(origin, direction, 100f, out var hit))
{
    FrinkyLog.Info($"Hit {hit.Entity.Name} at {hit.Point}, distance {hit.Distance}");
    // hit.Normal is the surface normal at the impact point
}

// All hits
var hits = Physics.RaycastAll(origin, direction, 100f);
foreach (var h in hits)
{
    FrinkyLog.Info($"Hit {h.Entity.Name} at distance {h.Distance}");
}
```

### Point-to-Point Raycasting

Cast between two world-space positions instead of specifying direction and distance:

```csharp
if (Physics.Raycast(pointA, pointB, out var hit))
{
    FrinkyLog.Info($"Something between A and B: {hit.Entity.Name}");
}

var hits = Physics.RaycastAll(pointA, pointB);
```

### RaycastParams

Use `RaycastParams` to filter raycast results:

```csharp
var rayParams = new RaycastParams
{
    IncludeTriggers = true,                       // include trigger colliders (skipped by default)
    IgnoredEntities = new HashSet<Entity> { Entity } // skip specific entities
};

if (Physics.Raycast(origin, direction, 100f, out var hit, rayParams))
{
    // hit.Entity will never be this entity
}
```

#### Ignoring an Entity Tree

`IgnoreEntityTree` collects the full hierarchy (root and all descendants) of a given entity into the ignore set. This is useful for ignoring the caster and all of its children/parents:

```csharp
var rayParams = new RaycastParams();
rayParams.IgnoreEntityTree(Entity); // ignores root parent + entire subtree

if (Physics.Raycast(origin, direction, 100f, out var hit, rayParams))
{
    // won't hit any entity in the same hierarchy tree
}
```

`RaycastHit` fields:

| Field | Type | Description |
|-------|------|-------------|
| `Entity` | `Entity` | The entity whose collider was hit |
| `Point` | `Vector3` | World-space impact point |
| `Normal` | `Vector3` | Surface normal at impact |
| `Distance` | `float` | Distance from ray origin to hit |

## Character Controller

A dynamic character controller backed by BEPU support constraints. Minimum setup on one entity:

1. `RigidbodyComponent` with `MotionType = Dynamic`
2. `CapsuleColliderComponent` (must be the first enabled collider)
3. `CharacterControllerComponent`

### Script-Side Input Methods

| Method | Style | Description |
|--------|-------|-------------|
| `AddMovementInput(direction)` | Unreal-style | Add world-space movement input |
| `Jump()` | Unreal-style | Request a jump |
| `SetMoveInput(Vector2)` | Direct | Set planar input directly |
| `MoveAndSlide(desiredVelocity, requestJump)` | Godot-style | Convenience all-in-one |

Or use `SimplePlayerInputComponent` for built-in WASD + mouse look with configurable keys.

### Key Properties

| Property | Default | Description |
|----------|---------|-------------|
| `MoveSpeed` | 4 | Movement speed |
| `JumpVelocity` | 6 | Jump impulse |
| `MaxSlopeDegrees` | 45 | Maximum walkable slope angle |
| `CrouchHeightScale` | 0.5 | Capsule height multiplier when crouching |
| `CrouchSpeedScale` | 0.5 | Speed multiplier when crouching |

## Crouching

The character controller has built-in crouch support:

- `Crouch()` / `Stand()` / `SetCrouching(bool)` — control crouch state from scripts
- Crouching shrinks the capsule height by `CrouchHeightScale` (default 50%) and reduces move speed by `CrouchSpeedScale` (default 50%)
- The entity position is adjusted to keep feet on the ground
- Velocity is preserved through the physics body rebuild that occurs during capsule resizing

`SimplePlayerInputComponent` provides automatic crouch handling with Left Ctrl (configurable via `CrouchKey`), including camera height blending:

| Property | Default | Description |
|----------|---------|-------------|
| `CrouchKey` | LeftControl | Key to hold for crouching |
| `AdjustCameraOnCrouch` | true | Blend camera height on crouch |
| `CrouchCameraYOffset` | -0.8 | Camera offset when crouched |
| `CameraOffsetLerpSpeed` | 10.0 | Camera blend speed (units/sec) |

## Physics Hitbox Preview

Press `F8` in the editor to toggle a wireframe overlay of all collider shapes.
