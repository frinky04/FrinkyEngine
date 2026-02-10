# CylinderPrimitive

Namespace: FrinkyEngine.Core.Components

A procedural cylinder primitive with configurable radius, height, and tessellation.

```csharp
public class CylinderPrimitive : PrimitiveComponent
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Component](./frinkyengine.core.ecs.component) → [RenderableComponent](./frinkyengine.core.components.renderablecomponent) → [PrimitiveComponent](./frinkyengine.core.components.primitivecomponent) → [CylinderPrimitive](./frinkyengine.core.components.cylinderprimitive)<br>
Attributes [ComponentCategoryAttribute](./frinkyengine.core.ecs.componentcategoryattribute), [ComponentDisplayNameAttribute](./frinkyengine.core.ecs.componentdisplaynameattribute)

## Properties

### **Radius**

Cylinder radius (defaults to 0.5).

```csharp
public float Radius { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **Height**

Cylinder height along the Y axis (defaults to 2).

```csharp
public float Height { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **Slices**

Number of circumferential slices for tessellation (defaults to 16).

```csharp
public int Slices { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

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

### **CylinderPrimitive()**

```csharp
public CylinderPrimitive()
```

## Methods

### **CreateMesh()**

```csharp
protected Mesh CreateMesh()
```

#### Returns

Mesh<br>
