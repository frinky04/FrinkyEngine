# PrimitiveComponent

Namespace: FrinkyEngine.Core.Components

Abstract base class for procedurally generated mesh primitives (cubes, spheres, etc.).
 Handles mesh generation, material assignment, and automatic rebuilds when properties change.

```csharp
public abstract class PrimitiveComponent : RenderableComponent
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Component](./frinkyengine.core.ecs.component) → [RenderableComponent](./frinkyengine.core.components.renderablecomponent) → [PrimitiveComponent](./frinkyengine.core.components.primitivecomponent)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **Material**

Material configuration for this primitive.

```csharp
public Material Material { get; set; }
```

#### Property Value

[Material](./frinkyengine.core.components.material)<br>

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

### **PrimitiveComponent()**

```csharp
protected PrimitiveComponent()
```

## Methods

### **Invalidate()**

```csharp
public void Invalidate()
```

### **CreateMesh()**

Creates the procedural mesh for this primitive. Subclasses implement this to define their geometry.

```csharp
protected abstract Mesh CreateMesh()
```

#### Returns

Mesh<br>
The generated .

### **MarkMeshDirty()**

Flags the mesh as needing a rebuild, triggering regeneration on the next frame.

```csharp
protected void MarkMeshDirty()
```

### **Start()**

```csharp
public void Start()
```

### **OnDestroy()**

```csharp
public void OnDestroy()
```
