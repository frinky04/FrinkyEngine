# MaterialSlot

Namespace: FrinkyEngine.Core.Components

Configures the material for a single slot on a [MeshRendererComponent](./frinkyengine.core.components.meshrenderercomponent).

```csharp
public class MaterialSlot
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [MaterialSlot](./frinkyengine.core.components.materialslot)

## Properties

### **MaterialType**

Which material mapping mode this slot uses (defaults to [MaterialType.SolidColor](./frinkyengine.core.rendering.materialtype#solidcolor)).

```csharp
public MaterialType MaterialType { get; set; }
```

#### Property Value

[MaterialType](./frinkyengine.core.rendering.materialtype)<br>

### **TexturePath**

Asset-relative path to the texture file, used when [MaterialSlot.MaterialType](./frinkyengine.core.components.materialslot#materialtype) is
 [MaterialType.Textured](./frinkyengine.core.rendering.materialtype#textured) or [MaterialType.TriplanarTexture](./frinkyengine.core.rendering.materialtype#triplanartexture).

```csharp
public AssetReference TexturePath { get; set; }
```

#### Property Value

[AssetReference](./frinkyengine.core.assets.assetreference)<br>

### **TriplanarScale**

Texture coordinate scale used when [MaterialSlot.MaterialType](./frinkyengine.core.components.materialslot#materialtype) is [MaterialType.TriplanarTexture](./frinkyengine.core.rendering.materialtype#triplanartexture).

```csharp
public float TriplanarScale { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **TriplanarBlendSharpness**

Blend sharpness used when [MaterialSlot.MaterialType](./frinkyengine.core.components.materialslot#materialtype) is [MaterialType.TriplanarTexture](./frinkyengine.core.rendering.materialtype#triplanartexture).
 Higher values produce harder transitions between projection axes.

```csharp
public float TriplanarBlendSharpness { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **TriplanarUseWorldSpace**

Whether triplanar projection uses world space (`true`) or object space (`false`).
 Used when [MaterialSlot.MaterialType](./frinkyengine.core.components.materialslot#materialtype) is [MaterialType.TriplanarTexture](./frinkyengine.core.rendering.materialtype#triplanartexture).

```csharp
public bool TriplanarUseWorldSpace { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

## Constructors

### **MaterialSlot()**

```csharp
public MaterialSlot()
```
