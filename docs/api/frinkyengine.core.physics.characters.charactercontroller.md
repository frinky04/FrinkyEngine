# CharacterController

Namespace: FrinkyEngine.Core.Physics.Characters

Raw data for a dynamic character controller instance.

```csharp
public struct CharacterController
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [CharacterController](./frinkyengine.core.physics.characters.charactercontroller)

## Fields

### **ViewDirection**

Direction the character is looking in world space. Defines the forward direction for movement.

```csharp
public Vector3 ViewDirection;
```

### **TargetVelocity**

Target horizontal velocity. 
 X component refers to desired velocity along the strafing direction (perpendicular to the view direction projected down to the surface), 
 Y component refers to the desired velocity along the forward direction (aligned with the view direction projected down to the surface).

```csharp
public Vector2 TargetVelocity;
```

### **TryJump**

If true, the character will try to jump on the next time step. Will be reset to false after being processed.

```csharp
public bool TryJump;
```

### **BodyHandle**

Handle of the body associated with the character.

```csharp
public BodyHandle BodyHandle;
```

### **LocalUp**

Character's up direction in the local space of the character's body.

```csharp
public Vector3 LocalUp;
```

### **JumpVelocity**

Velocity at which the character pushes off the support during a jump.

```csharp
public float JumpVelocity;
```

### **MaximumHorizontalForce**

Maximum force the character can apply tangent to the supporting surface to move.

```csharp
public float MaximumHorizontalForce;
```

### **MaximumVerticalForce**

Maximum force the character can apply to glue itself to the supporting surface.

```csharp
public float MaximumVerticalForce;
```

### **CosMaximumSlope**

Cosine of the maximum slope angle that the character can treat as a support.

```csharp
public float CosMaximumSlope;
```

### **MinimumSupportDepth**

Depth threshold beyond which a contact is considered a support if it the normal allows it.

```csharp
public float MinimumSupportDepth;
```

### **MinimumSupportContinuationDepth**

Depth threshold beyond which a contact is considered a support if the previous frame had support, even if it isn't deep enough to meet the MinimumSupportDepth.

```csharp
public float MinimumSupportContinuationDepth;
```

### **Supported**

Whether the character is currently supported.

```csharp
public bool Supported;
```

### **Support**

Collidable supporting the character, if any. Only valid if Supported is true.

```csharp
public CollidableReference Support;
```

### **MotionConstraintHandle**

Handle of the character's motion constraint, if any. Only valid if Supported is true.

```csharp
public ConstraintHandle MotionConstraintHandle;
```
