# PrimitiveComponent

Namespace: FrinkyEngine.Core.Components

Abstract base class for procedurally generated mesh primitives (cubes, spheres, etc.).
 Handles mesh generation, material assignment, and automatic rebuilds when properties change.

```csharp
public abstract class PrimitiveComponent : RenderableComponent
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Component](./frinkyengine.core.ecs.component) → [RenderableComponent](./frinkyengine.core.components.renderablecomponent) → [PrimitiveComponent](./frinkyengine.core.components.primitivecomponent)

## Properties

### **MaterialType**

Which material mapping mode the primitive uses (defaults to [MaterialType.SolidColor](./frinkyengine.core.rendering.materialtype#solidcolor)).

```csharp
public MaterialType MaterialType { get; set; }
```

#### Property Value

[MaterialType](./frinkyengine.core.rendering.materialtype)<br>

### **TexturePath**

Asset-relative path to the texture file, used when [PrimitiveComponent.MaterialType](./frinkyengine.core.components.primitivecomponent#materialtype) is
 [MaterialType.Textured](./frinkyengine.core.rendering.materialtype#textured) or [MaterialType.TriplanarTexture](./frinkyengine.core.rendering.materialtype#triplanartexture).

```csharp
public AssetReference TexturePath { get; set; }
```

#### Property Value

[AssetReference](./frinkyengine.core.assets.assetreference)<br>

### **TriplanarScale**

Triplanar projection scale, used when [PrimitiveComponent.MaterialType](./frinkyengine.core.components.primitivecomponent#materialtype) is [MaterialType.TriplanarTexture](./frinkyengine.core.rendering.materialtype#triplanartexture).

```csharp
public float TriplanarScale { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **TriplanarBlendSharpness**

Triplanar axis blend sharpness, used when [PrimitiveComponent.MaterialType](./frinkyengine.core.components.primitivecomponent#materialtype) is [MaterialType.TriplanarTexture](./frinkyengine.core.rendering.materialtype#triplanartexture).

```csharp
public float TriplanarBlendSharpness { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **TriplanarUseWorldSpace**

Whether triplanar projection uses world-space coordinates (`true`) or object-space coordinates (`false`).
 Used when [PrimitiveComponent.MaterialType](./frinkyengine.core.components.primitivecomponent#materialtype) is [MaterialType.TriplanarTexture](./frinkyengine.core.rendering.materialtype#triplanartexture).

```csharp
public bool TriplanarUseWorldSpace { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Tint**

Color multiplier applied when drawing this renderable (defaults to white / fully opaque).

```csharp
public Color Tint { get; set; }
```

#### Property Value

Color<br>

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
