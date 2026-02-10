# RigidbodyComponent

Namespace: FrinkyEngine.Core.Components

Physics body component used by the scene physics system.

```csharp
public class RigidbodyComponent : FrinkyEngine.Core.ECS.Component
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Component](./frinkyengine.core.ecs.component) → [RigidbodyComponent](./frinkyengine.core.components.rigidbodycomponent)<br>
Attributes [ComponentCategoryAttribute](./frinkyengine.core.ecs.componentcategoryattribute)

## Properties

### **MotionType**

How the body should be simulated.

```csharp
public BodyMotionType MotionType { get; set; }
```

#### Property Value

[BodyMotionType](./frinkyengine.core.components.bodymotiontype)<br>

### **Mass**

Body mass used when [RigidbodyComponent.MotionType](./frinkyengine.core.components.rigidbodycomponent#motiontype) is [BodyMotionType.Dynamic](./frinkyengine.core.components.bodymotiontype#dynamic).

```csharp
public float Mass { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **LinearDamping**

Fraction of linear velocity removed per second in range [0, 1].

```csharp
public float LinearDamping { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **AngularDamping**

Fraction of angular velocity removed per second in range [0, 1].

```csharp
public float AngularDamping { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **ContinuousDetection**

Enables continuous collision detection for the body collidable.

```csharp
public bool ContinuousDetection { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **LockPositionX**

Locks translation on world X.

```csharp
public bool LockPositionX { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **LockPositionY**

Locks translation on world Y.

```csharp
public bool LockPositionY { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **LockPositionZ**

Locks translation on world Z.

```csharp
public bool LockPositionZ { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **LockRotationX**

Locks rotation around world X.

```csharp
public bool LockRotationX { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **LockRotationY**

Locks rotation around world Y.

```csharp
public bool LockRotationY { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **LockRotationZ**

Locks rotation around world Z.

```csharp
public bool LockRotationZ { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **InterpolationMode**

Controls render interpolation behavior for this body.

```csharp
public RigidbodyInterpolationMode InterpolationMode { get; set; }
```

#### Property Value

[RigidbodyInterpolationMode](./frinkyengine.core.components.rigidbodyinterpolationmode)<br>

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

### **RigidbodyComponent()**

```csharp
public RigidbodyComponent()
```

## Methods

### **ApplyForce(Vector3)**

Adds a force that will be applied during the next physics step.

```csharp
public void ApplyForce(Vector3 force)
```

#### Parameters

`force` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

### **ApplyImpulse(Vector3)**

Adds an instantaneous linear impulse.

```csharp
public void ApplyImpulse(Vector3 impulse)
```

#### Parameters

`impulse` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

### **SetLinearVelocity(Vector3)**

Sets the rigidbody linear velocity.

```csharp
public void SetLinearVelocity(Vector3 velocity)
```

#### Parameters

`velocity` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

### **Teleport(Vector3, Quaternion, Boolean)**

Teleports the rigidbody to a new pose without interpolating across the movement.

```csharp
public void Teleport(Vector3 position, Quaternion rotation, bool resetVelocity)
```

#### Parameters

`position` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>
Target local position.

`rotation` [Quaternion](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.quaternion)<br>
Target local rotation.

`resetVelocity` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
If true, clears linear and angular velocity.

### **GetLinearVelocity()**

Gets the current linear velocity.

```csharp
public Vector3 GetLinearVelocity()
```

#### Returns

[Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

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
