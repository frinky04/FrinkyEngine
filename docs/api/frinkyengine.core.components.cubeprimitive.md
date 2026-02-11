# CubePrimitive

Namespace: FrinkyEngine.Core.Components

A procedural box primitive with configurable width, height, and depth.

```csharp
public class CubePrimitive : PrimitiveComponent
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Component](./frinkyengine.core.ecs.component) → [RenderableComponent](./frinkyengine.core.components.renderablecomponent) → [PrimitiveComponent](./frinkyengine.core.components.primitivecomponent) → [CubePrimitive](./frinkyengine.core.components.cubeprimitive)<br>
Attributes [ComponentCategoryAttribute](./frinkyengine.core.ecs.componentcategoryattribute), [ComponentDisplayNameAttribute](./frinkyengine.core.ecs.componentdisplaynameattribute)

## Properties

### **Width**

Size along the X axis (defaults to 1).

```csharp
public float Width { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **Height**

Size along the Y axis (defaults to 1).

```csharp
public float Height { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **Depth**

Size along the Z axis (defaults to 1).

```csharp
public float Depth { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

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

### **CubePrimitive()**

```csharp
public CubePrimitive()
```

## Methods

### **CreateMesh()**

```csharp
protected Mesh CreateMesh()
```

#### Returns

Mesh<br>
