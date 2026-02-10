# CharacterControllerComponent

Namespace: FrinkyEngine.Core.Components

High-level dynamic character locomotion component backed by BEPU's character support constraints.
 Requires an enabled [RigidbodyComponent](./frinkyengine.core.components.rigidbodycomponent) and [CapsuleColliderComponent](./frinkyengine.core.components.capsulecollidercomponent) on the same entity.

```csharp
public class CharacterControllerComponent : FrinkyEngine.Core.ECS.Component
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Component](./frinkyengine.core.ecs.component) → [CharacterControllerComponent](./frinkyengine.core.components.charactercontrollercomponent)<br>
Attributes [ComponentCategoryAttribute](./frinkyengine.core.ecs.componentcategoryattribute)

## Properties

### **MoveSpeed**

Maximum supported horizontal speed for input-driven movement.

```csharp
public float MoveSpeed { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **JumpVelocity**

Upward launch speed used when a jump is requested while supported.

```csharp
public float JumpVelocity { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **MaxSlopeDegrees**

Maximum walkable slope angle in degrees.

```csharp
public float MaxSlopeDegrees { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **MaximumHorizontalForce**

Maximum horizontal force applied by support constraints.

```csharp
public float MaximumHorizontalForce { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **MaximumVerticalForce**

Maximum vertical force used to maintain support contact.

```csharp
public float MaximumVerticalForce { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **AirControlForceScale**

Air control acceleration force scale applied while unsupported.

```csharp
public float AirControlForceScale { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **AirControlSpeedScale**

Air control speed cap scale relative to desired movement speed.

```csharp
public float AirControlSpeedScale { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **UseEntityForwardAsViewDirection**

When true, view direction is read from [TransformComponent.Forward](./frinkyengine.core.components.transformcomponent#forward).

```csharp
public bool UseEntityForwardAsViewDirection { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **ViewDirectionOverride**

View direction used when [CharacterControllerComponent.UseEntityForwardAsViewDirection](./frinkyengine.core.components.charactercontrollercomponent#useentityforwardasviewdirection) is false.

```csharp
public Vector3 ViewDirectionOverride { get; set; }
```

#### Property Value

[Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

### **Supported**

True when the character is currently supported by a walkable contact.

```csharp
public bool Supported { get; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **LastComputedTargetVelocity**

Last target horizontal world velocity computed from input this frame.

```csharp
public Vector3 LastComputedTargetVelocity { get; }
```

#### Property Value

[Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

### **IsCrouching**

True when the character is currently in a crouched state.

```csharp
public bool IsCrouching { get; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **CrouchHeightScale**

Scale applied to capsule height when crouching (0.1-1.0).
 Default is 0.5 (50% of standing height).

```csharp
public float CrouchHeightScale { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **CrouchSpeedScale**

Speed multiplier applied when crouching (0.0-1.0).
 Default is 0.5 (50% of normal move speed).

```csharp
public float CrouchSpeedScale { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **Entity**

The [Entity](./frinkyengine.core.ecs.entity) this component is attached to.

```csharp
public Entity Entity { get; internal set; }
```

#### Property Value

[Entity](./frinkyengine.core.ecs.entity)<br>

### **Enabled**

Whether this component is active. Disabled components skip [Component.Update(Single)](./frinkyengine.core.ecs.component#updatesingle) and [Component.LateUpdate(Single)](./frinkyengine.core.ecs.component#lateupdatesingle).

```csharp
public bool Enabled { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **EditorOnly**

When `true`, this component is only active in the editor and is skipped during runtime play.

```csharp
public bool EditorOnly { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **HasStarted**

Indicates whether [Component.Start()](./frinkyengine.core.ecs.component#start) has already been called on this component.

```csharp
public bool HasStarted { get; internal set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

## Constructors

### **CharacterControllerComponent()**

```csharp
public CharacterControllerComponent()
```

## Methods

### **AddMovementInput(Vector3, Single)**

Unreal-style movement input accumulator.

```csharp
public void AddMovementInput(Vector3 worldDirection, float scale)
```

#### Parameters

`worldDirection` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

`scale` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **SetMoveInput(Vector2)**

Sets direct planar movement input (X = strafe, Y = forward).

```csharp
public void SetMoveInput(Vector2 input)
```

#### Parameters

`input` [Vector2](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector2)<br>

### **Jump()**

Requests a jump attempt on the next physics step.

```csharp
public void Jump()
```

### **MoveAndSlide(Vector3, Boolean)**

Godot-style convenience call for setting desired movement velocity and optional jump request.

```csharp
public void MoveAndSlide(Vector3 desiredWorldVelocity, bool requestJump)
```

#### Parameters

`desiredWorldVelocity` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

`requestJump` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **IsOnFloor()**

Returns true when the character has support contact.

```csharp
public bool IsOnFloor()
```

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **GetVelocity()**

Returns the current linear velocity of the attached rigidbody, if available.

```csharp
public Vector3 GetVelocity()
```

#### Returns

[Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

### **Crouch()**

Enters crouch state, reducing capsule height and movement speed.

```csharp
public void Crouch()
```

### **Stand()**

Exits crouch state, restoring capsule height and movement speed.

```csharp
public void Stand()
```

### **SetCrouching(Boolean)**

Sets the crouch state directly.

```csharp
public void SetCrouching(bool shouldCrouch)
```

#### Parameters

`shouldCrouch` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **OnEnable()**

```csharp
public void OnEnable()
```

### **OnDisable()**

```csharp
public void OnDisable()
```

### **OnDestroy()**

```csharp
public void OnDestroy()
```
