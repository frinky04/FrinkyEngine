# BloomEffect

Namespace: FrinkyEngine.Core.Rendering.PostProcessing.Effects

Multi-pass bloom effect: threshold extraction, iterative downsample/upsample, and additive composite.
 Creates a glow around bright areas of the image.

```csharp
public class BloomEffect : FrinkyEngine.Core.Rendering.PostProcessing.PostProcessEffect, System.IDisposable
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [FObject](./frinkyengine.core.ecs.fobject) → [PostProcessEffect](./frinkyengine.core.rendering.postprocessing.postprocesseffect) → [BloomEffect](./frinkyengine.core.rendering.postprocessing.effects.bloomeffect)<br>
Implements [IDisposable](https://docs.microsoft.com/en-us/dotnet/api/system.idisposable)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **DisplayName**

```csharp
public string DisplayName { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **Threshold**

Brightness threshold above which pixels contribute to bloom.

```csharp
public float Threshold { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **SoftKnee**

Softness of the threshold transition (0 = hard, 1 = very soft).

```csharp
public float SoftKnee { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **Intensity**

Strength of the bloom added back to the scene.

```csharp
public float Intensity { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **Iterations**

Number of downsample/upsample iterations (more = wider bloom, but more cost).

```csharp
public int Iterations { get; set; }
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

### **BloomEffect()**

```csharp
public BloomEffect()
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
