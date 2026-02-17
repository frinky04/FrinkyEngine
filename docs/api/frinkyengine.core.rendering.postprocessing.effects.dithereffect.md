# DitherEffect

Namespace: FrinkyEngine.Core.Rendering.PostProcessing.Effects

PSX-style 4x4 Bayer ordered dithering effect. Quantizes color depth and applies
 a tiled dither pattern for a retro look.

```csharp
public class DitherEffect : FrinkyEngine.Core.Rendering.PostProcessing.PostProcessEffect, System.IDisposable
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [FObject](./frinkyengine.core.ecs.fobject) → [PostProcessEffect](./frinkyengine.core.rendering.postprocessing.postprocesseffect) → [DitherEffect](./frinkyengine.core.rendering.postprocessing.effects.dithereffect)<br>
Implements [IDisposable](https://docs.microsoft.com/en-us/dotnet/api/system.idisposable)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **DisplayName**

```csharp
public string DisplayName { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **ColorLevels**

Number of color levels per channel. Lower values produce a more retro look (e.g. 32 for PSX-style).

```csharp
public float ColorLevels { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **DitherStrength**

Blend factor between original and dithered output (0 = original, 1 = fully dithered).

```csharp
public float DitherStrength { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

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

### **DitherEffect()**

```csharp
public DitherEffect()
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
