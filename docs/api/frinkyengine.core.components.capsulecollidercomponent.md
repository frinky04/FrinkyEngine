# CapsuleColliderComponent

Namespace: FrinkyEngine.Core.Components

Capsule collider aligned to the entity local Y axis.

```csharp
public class CapsuleColliderComponent : ColliderComponent
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Component](./frinkyengine.core.ecs.component) → [ColliderComponent](./frinkyengine.core.components.collidercomponent) → [CapsuleColliderComponent](./frinkyengine.core.components.capsulecollidercomponent)<br>
Attributes [ComponentCategoryAttribute](./frinkyengine.core.ecs.componentcategoryattribute), [InspectorMessageIfAttribute](./frinkyengine.core.ecs.inspectormessageifattribute)

## Properties

### **Radius**

Radius of the capsule hemispheres.

```csharp
public float Radius { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **Length**

Length of the cylindrical section between hemispherical caps.

```csharp
public float Length { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **Friction**

Surface friction coefficient used during collision response.

```csharp
public float Friction { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **Restitution**

Bounciness value in range [0, 1].

```csharp
public float Restitution { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **IsTrigger**

When `true`, this collider acts as a trigger volume: overlap events fire but no physical response occurs.

```csharp
public bool IsTrigger { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Center**

Local offset applied to the collider relative to the entity transform.

```csharp
public Vector3 Center { get; set; }
```

#### Property Value

[Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

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

### **CapsuleColliderComponent()**

```csharp
public CapsuleColliderComponent()
```
