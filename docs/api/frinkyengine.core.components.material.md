# Material

Namespace: FrinkyEngine.Core.Components

Configures the material for a mesh surface.
 Used by [MeshRendererComponent](./frinkyengine.core.components.meshrenderercomponent) (multiple slots) and [PrimitiveComponent](./frinkyengine.core.components.primitivecomponent) (single material).

```csharp
public class Material
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Material](./frinkyengine.core.components.material)

## Properties

### **Tint**

Color multiplier applied to this material's surface (defaults to white / fully opaque).

```csharp
public Color Tint { get; set; }
```

#### Property Value

Color<br>

### **MaterialType**

Which material mapping mode this material uses (defaults to [MaterialType.SolidColor](./frinkyengine.core.rendering.materialtype#solidcolor)).

```csharp
public MaterialType MaterialType { get; set; }
```

#### Property Value

[MaterialType](./frinkyengine.core.rendering.materialtype)<br>

### **TexturePath**

Asset-relative path to the texture file, used when [Material.MaterialType](./frinkyengine.core.components.material#materialtype) is
 [MaterialType.Textured](./frinkyengine.core.rendering.materialtype#textured) or [MaterialType.TriplanarTexture](./frinkyengine.core.rendering.materialtype#triplanartexture).

```csharp
public AssetReference TexturePath { get; set; }
```

#### Property Value

[AssetReference](./frinkyengine.core.assets.assetreference)<br>

### **TriplanarScale**

Texture coordinate scale used when [Material.MaterialType](./frinkyengine.core.components.material#materialtype) is [MaterialType.TriplanarTexture](./frinkyengine.core.rendering.materialtype#triplanartexture).

```csharp
public float TriplanarScale { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **TriplanarBlendSharpness**

Blend sharpness used when [Material.MaterialType](./frinkyengine.core.components.material#materialtype) is [MaterialType.TriplanarTexture](./frinkyengine.core.rendering.materialtype#triplanartexture).
 Higher values produce harder transitions between projection axes.

```csharp
public float TriplanarBlendSharpness { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **TriplanarUseWorldSpace**

Whether triplanar projection uses world space (`true`) or object space (`false`).
 Used when [Material.MaterialType](./frinkyengine.core.components.material#materialtype) is [MaterialType.TriplanarTexture](./frinkyengine.core.rendering.materialtype#triplanartexture).

```csharp
public bool TriplanarUseWorldSpace { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

## Constructors

### **Material()**

```csharp
public Material()
```
