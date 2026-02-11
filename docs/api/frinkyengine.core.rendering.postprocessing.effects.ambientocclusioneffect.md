# AmbientOcclusionEffect

Namespace: FrinkyEngine.Core.Rendering.PostProcessing.Effects

Screen-space ambient occlusion (SSAO) effect that darkens creases and corners.
 Uses view-space hemisphere sampling with perspective-correct reprojection and
 separable bilateral blur weighted by depth for smooth, edge-preserving results.

```csharp
public class AmbientOcclusionEffect : FrinkyEngine.Core.Rendering.PostProcessing.PostProcessEffect, System.IDisposable
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [FObject](./frinkyengine.core.ecs.fobject) → [PostProcessEffect](./frinkyengine.core.rendering.postprocessing.postprocesseffect) → [AmbientOcclusionEffect](./frinkyengine.core.rendering.postprocessing.effects.ambientocclusioneffect)<br>
Implements [IDisposable](https://docs.microsoft.com/en-us/dotnet/api/system.idisposable)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **DisplayName**

```csharp
public string DisplayName { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **NeedsDepth**

```csharp
public bool NeedsDepth { get; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Radius**

Sampling radius in world-space units. Higher values detect occlusion over a wider area.

```csharp
public float Radius { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **Intensity**

Strength of the occlusion darkening.

```csharp
public float Intensity { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **Bias**

Depth bias in world-space units to prevent self-occlusion artifacts.

```csharp
public float Bias { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **SampleCount**

Number of hemisphere samples per pixel (max 64).

```csharp
public int SampleCount { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **BlurSize**

Size of the bilateral blur kernel (half-extent in pixels).

```csharp
public int BlurSize { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **Enabled**

Whether this effect is active. Disabled effects are skipped during rendering.

```csharp
public bool Enabled { get; set; }
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

### **AmbientOcclusionEffect()**

```csharp
public AmbientOcclusionEffect()
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
