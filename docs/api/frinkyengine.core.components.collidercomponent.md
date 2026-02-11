# ColliderComponent

Namespace: FrinkyEngine.Core.Components

Base component for all physics collider shapes.

```csharp
public abstract class ColliderComponent : FrinkyEngine.Core.ECS.Component
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Component](./frinkyengine.core.ecs.component) → [ColliderComponent](./frinkyengine.core.components.collidercomponent)<br>
Attributes [InspectorMessageIfAttribute](./frinkyengine.core.ecs.inspectormessageifattribute)

## Properties

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

### **ColliderComponent()**

```csharp
protected ColliderComponent()
```

## Methods

### **MarkColliderDirty()**

Marks collider data as changed so physics can rebuild representation if needed.

```csharp
protected void MarkColliderDirty()
```

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
