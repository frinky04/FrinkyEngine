# TransformComponent

Namespace: FrinkyEngine.Core.Components

Defines an entity's position, rotation, and scale in 3D space.
 Supports parent-child hierarchies for nested transforms.

```csharp
public class TransformComponent : FrinkyEngine.Core.ECS.Component
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Component](./frinkyengine.core.ecs.component) → [TransformComponent](./frinkyengine.core.components.transformcomponent)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

**Remarks:**

Every [Entity](./frinkyengine.core.ecs.entity) always has exactly one [TransformComponent](./frinkyengine.core.components.transformcomponent) that cannot be removed.
 Local properties are relative to the parent transform; world properties account for the full hierarchy.

## Properties

### **LocalPosition**

Position relative to the parent transform (or world origin if no parent).

```csharp
public Vector3 LocalPosition { get; set; }
```

#### Property Value

[Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

### **LocalRotation**

Rotation relative to the parent transform as a quaternion.

```csharp
public Quaternion LocalRotation { get; set; }
```

#### Property Value

[Quaternion](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.quaternion)<br>

### **LocalScale**

Scale relative to the parent transform (defaults to `(1, 1, 1)`).

```csharp
public Vector3 LocalScale { get; set; }
```

#### Property Value

[Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

### **EulerAngles**

Local rotation expressed as Euler angles in degrees (X = pitch, Y = yaw, Z = roll).

```csharp
public Vector3 EulerAngles { get; set; }
```

#### Property Value

[Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

### **Parent**

The parent transform in the hierarchy, or `null` if this is a root transform.

```csharp
public TransformComponent Parent { get; }
```

#### Property Value

[TransformComponent](./frinkyengine.core.components.transformcomponent)<br>

### **Children**

The immediate child transforms in the hierarchy.

```csharp
public IReadOnlyList<TransformComponent> Children { get; }
```

#### Property Value

[IReadOnlyList&lt;TransformComponent&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1)<br>

### **WorldPosition**

The position in world space, computed from the full hierarchy.

```csharp
public Vector3 WorldPosition { get; set; }
```

#### Property Value

[Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

### **LocalMatrix**

The local transform matrix (scale * rotation * translation).

```csharp
public Matrix4x4 LocalMatrix { get; }
```

#### Property Value

[Matrix4x4](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.matrix4x4)<br>

### **WorldMatrix**

The world transform matrix, combining this transform with all parent transforms.

```csharp
public Matrix4x4 WorldMatrix { get; }
```

#### Property Value

[Matrix4x4](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.matrix4x4)<br>

### **Forward**

The forward direction (negative Z axis) in world space.

```csharp
public Vector3 Forward { get; }
```

#### Property Value

[Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

### **Right**

The right direction (positive X axis) in world space.

```csharp
public Vector3 Right { get; }
```

#### Property Value

[Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

### **Up**

The up direction (positive Y axis) in world space.

```csharp
public Vector3 Up { get; }
```

#### Property Value

[Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

### **WorldRotation**

The rotation in world space, combining local rotation with all parent rotations.

```csharp
public Quaternion WorldRotation { get; set; }
```

#### Property Value

[Quaternion](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.quaternion)<br>

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

### **TransformComponent()**

```csharp
public TransformComponent()
```

## Methods

### **SetParent(TransformComponent)**

Sets a new parent for this transform, updating the hierarchy.

```csharp
public void SetParent(TransformComponent newParent)
```

#### Parameters

`newParent` [TransformComponent](./frinkyengine.core.components.transformcomponent)<br>
The new parent transform, or `null` to make this a root transform.

**Remarks:**

Circular hierarchies are prevented — if this transform is an ancestor of `newParent`, the call is ignored.

### **TransformPoint(Vector3)**

Transforms a point from local space to world space.

```csharp
public Vector3 TransformPoint(Vector3 point)
```

#### Parameters

`point` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

#### Returns

[Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

### **InverseTransformPoint(Vector3)**

Transforms a point from world space to local space.

```csharp
public Vector3 InverseTransformPoint(Vector3 point)
```

#### Parameters

`point` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

#### Returns

[Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

### **TransformDirection(Vector3)**

Transforms a direction from local space to world space (rotation only, ignores scale).

```csharp
public Vector3 TransformDirection(Vector3 direction)
```

#### Parameters

`direction` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

#### Returns

[Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

### **InverseTransformDirection(Vector3)**

Transforms a direction from world space to local space (rotation only, ignores scale).

```csharp
public Vector3 InverseTransformDirection(Vector3 direction)
```

#### Parameters

`direction` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

#### Returns

[Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

### **TransformVector(Vector3)**

Transforms a vector from local space to world space (rotation and scale).

```csharp
public Vector3 TransformVector(Vector3 vector)
```

#### Parameters

`vector` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

#### Returns

[Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

### **InverseTransformVector(Vector3)**

Transforms a vector from world space to local space (rotation and scale).

```csharp
public Vector3 InverseTransformVector(Vector3 vector)
```

#### Parameters

`vector` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

#### Returns

[Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>
