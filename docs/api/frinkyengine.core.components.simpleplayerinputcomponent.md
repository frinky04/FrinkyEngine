# SimplePlayerInputComponent

Namespace: FrinkyEngine.Core.Components

Simple configurable player input component for movement and mouse look.
 Uses [CharacterControllerComponent](./frinkyengine.core.components.charactercontrollercomponent) when present; otherwise falls back to transform or rigidbody motion.

```csharp
public class SimplePlayerInputComponent : FrinkyEngine.Core.ECS.Component
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Component](./frinkyengine.core.ecs.component) → [SimplePlayerInputComponent](./frinkyengine.core.components.simpleplayerinputcomponent)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute), [ComponentCategoryAttribute](./frinkyengine.core.ecs.componentcategoryattribute)

## Properties

### **MoveForwardKey**

Key used to move forward. Defaults to .

```csharp
public KeyboardKey MoveForwardKey { get; set; }
```

#### Property Value

KeyboardKey<br>

### **MoveBackwardKey**

Key used to move backward. Defaults to .

```csharp
public KeyboardKey MoveBackwardKey { get; set; }
```

#### Property Value

KeyboardKey<br>

### **MoveLeftKey**

Key used to strafe left. Defaults to .

```csharp
public KeyboardKey MoveLeftKey { get; set; }
```

#### Property Value

KeyboardKey<br>

### **MoveRightKey**

Key used to strafe right. Defaults to .

```csharp
public KeyboardKey MoveRightKey { get; set; }
```

#### Property Value

KeyboardKey<br>

### **JumpKey**

Key used for jumping. Defaults to .

```csharp
public KeyboardKey JumpKey { get; set; }
```

#### Property Value

KeyboardKey<br>

### **EnableMouseLook**

Enables mouse-driven look rotation.

```csharp
public bool EnableMouseLook { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **RequireLookMouseButton**

If true, look rotation is only applied while [SimplePlayerInputComponent.LookMouseButton](./frinkyengine.core.components.simpleplayerinputcomponent#lookmousebutton) is held.

```csharp
public bool RequireLookMouseButton { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **LookMouseButton**

Mouse button gate for look input when [SimplePlayerInputComponent.RequireLookMouseButton](./frinkyengine.core.components.simpleplayerinputcomponent#requirelookmousebutton) is enabled.

```csharp
public MouseButton LookMouseButton { get; set; }
```

#### Property Value

MouseButton<br>

### **RotatePitch**

If true, applies pitch rotation around the local X axis.

```csharp
public bool RotatePitch { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **UseViewDirectionOverrideForCharacterLook**

If true, character look uses [CharacterControllerComponent.ViewDirectionOverride](./frinkyengine.core.components.charactercontrollercomponent#viewdirectionoverride)
 so pitch does not have to rotate the physics body.

```csharp
public bool UseViewDirectionOverrideForCharacterLook { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **ApplyPitchToCharacterBody**

If true and a character controller is active, pitch is applied to the entity transform.
 Keeping this false avoids tilting the character body.

```csharp
public bool ApplyPitchToCharacterBody { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **InvertMouseY**

Inverts mouse Y input for pitch rotation.

```csharp
public bool InvertMouseY { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **InvertMouseX**

Inverts mouse X input for yaw rotation.

```csharp
public bool InvertMouseX { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **MouseSensitivity**

Sensitivity multiplier applied to mouse delta.

```csharp
public float MouseSensitivity { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **MinPitchDegrees**

Lower clamp for pitch in degrees.

```csharp
public float MinPitchDegrees { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **MaxPitchDegrees**

Upper clamp for pitch in degrees.

```csharp
public float MaxPitchDegrees { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **UseCharacterController**

If true, forwards movement and jump to [CharacterControllerComponent](./frinkyengine.core.components.charactercontrollercomponent) when available.

```csharp
public bool UseCharacterController { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **AllowJump**

Allows jump key processing.

```csharp
public bool AllowJump { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **FallbackMoveSpeed**

Speed used by fallback motion when no character controller is present.

```csharp
public float FallbackMoveSpeed { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **FallbackJumpImpulse**

Upward impulse used for fallback rigidbody jumping.

```csharp
public float FallbackJumpImpulse { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **DriveAttachedCamera**

If true, a child camera entity is driven using janky follow/pitch behavior.

```csharp
public bool DriveAttachedCamera { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **AttachedCameraLocalOffset**

Local offset applied to the attached camera entity relative to the controller entity.

```csharp
public Vector3 AttachedCameraLocalOffset { get; set; }
```

#### Property Value

[Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

### **AttachedCameraBackDistance**

Additional camera distance along local +Z (behind the entity when forward is -Z).

```csharp
public float AttachedCameraBackDistance { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **CameraEntity**

Optional explicit reference to the camera entity. When set, takes priority over child entity search.

```csharp
public EntityReference CameraEntity { get; set; }
```

#### Property Value

[EntityReference](./frinkyengine.core.ecs.entityreference)<br>

### **CrouchKey**

Key used to crouch. Defaults to .

```csharp
public KeyboardKey CrouchKey { get; set; }
```

#### Property Value

KeyboardKey<br>

### **AdjustCameraOnCrouch**

If true, automatically adjusts [SimplePlayerInputComponent.AttachedCameraLocalOffset](./frinkyengine.core.components.simpleplayerinputcomponent#attachedcameralocaloffset) when crouching.

```csharp
public bool AdjustCameraOnCrouch { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **CameraOffsetLerpSpeed**

Speed of camera offset interpolation during crouch transitions.
 Higher values = faster transition. Default is 5.0.

```csharp
public float CameraOffsetLerpSpeed { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **StandingHeadHeight**

Camera height from feet when standing (in meters).
 Crouching height is automatically calculated using the character controller's crouch scale.
 Default is 1.6.

```csharp
public float StandingHeadHeight { get; set; }
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

### **SimplePlayerInputComponent()**

```csharp
public SimplePlayerInputComponent()
```

## Methods

### **Start()**

```csharp
public void Start()
```

### **Update(Single)**

```csharp
public void Update(float dt)
```

#### Parameters

`dt` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
