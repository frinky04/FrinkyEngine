# SpherePrimitive

Namespace: FrinkyEngine.Core.Components

A procedural sphere primitive with configurable radius and tessellation.

```csharp
public class SpherePrimitive : PrimitiveComponent
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Component](./frinkyengine.core.ecs.component) → [RenderableComponent](./frinkyengine.core.components.renderablecomponent) → [PrimitiveComponent](./frinkyengine.core.components.primitivecomponent) → [SpherePrimitive](./frinkyengine.core.components.sphereprimitive)<br>
Attributes [ComponentCategoryAttribute](./frinkyengine.core.ecs.componentcategoryattribute), [ComponentDisplayNameAttribute](./frinkyengine.core.ecs.componentdisplaynameattribute)

## Properties

### **Radius**

Sphere radius (defaults to 0.5).

```csharp
public float Radius { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **Rings**

Number of horizontal rings for tessellation (defaults to 16).

```csharp
public int Rings { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **Slices**

Number of vertical slices for tessellation (defaults to 16).

```csharp
public int Slices { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

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

### **SpherePrimitive()**

```csharp
public SpherePrimitive()
```

## Methods

### **CreateMesh()**

```csharp
protected Mesh CreateMesh()
```

#### Returns

Mesh<br>
