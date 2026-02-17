# PaletteEffect

Namespace: FrinkyEngine.Core.Rendering.PostProcessing.Effects

Snaps rendered colors to the nearest color in a loaded JASC-PAL palette file.
 Loads the palette as a 1D GPU texture and performs nearest-color matching in the fragment shader.

```csharp
public class PaletteEffect : FrinkyEngine.Core.Rendering.PostProcessing.PostProcessEffect, System.IDisposable
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [FObject](./frinkyengine.core.ecs.fobject) → [PostProcessEffect](./frinkyengine.core.rendering.postprocessing.postprocesseffect) → [PaletteEffect](./frinkyengine.core.rendering.postprocessing.effects.paletteeffect)<br>
Implements [IDisposable](https://docs.microsoft.com/en-us/dotnet/api/system.idisposable)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **DisplayName**

```csharp
public string DisplayName { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **PalettePath**

Asset reference to a .pal file (JASC-PAL format).

```csharp
public AssetReference PalettePath { get; set; }
```

#### Property Value

[AssetReference](./frinkyengine.core.assets.assetreference)<br>

### **Enabled**

Whether this effect is active. Disabled effects are skipped during rendering.

```csharp
public bool Enabled { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **NeedsDepth**

Override to return `true` if this effect requires the depth texture (e.g. fog, SSAO).

```csharp
public bool NeedsDepth { get; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **IsInitialized**

Indicates whether [PostProcessEffect.Initialize(String)](./frinkyengine.core.rendering.postprocessing.postprocesseffect#initializestring) has been called successfully.

```csharp
public bool IsInitialized { get; protected set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

## Constructors

### **PaletteEffect()**

```csharp
public PaletteEffect()
```

## Methods

### **Initialize(String)**

```csharp
public void Initialize(string shaderBasePath)
```

#### Parameters

`shaderBasePath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **Render(Texture2D, RenderTexture2D, PostProcessContext)**

```csharp
public void Render(Texture2D source, RenderTexture2D destination, PostProcessContext context)
```

#### Parameters

`source` Texture2D<br>

`destination` RenderTexture2D<br>

`context` [PostProcessContext](./frinkyengine.core.rendering.postprocessing.postprocesscontext)<br>

### **Dispose()**

```csharp
public void Dispose()
```
