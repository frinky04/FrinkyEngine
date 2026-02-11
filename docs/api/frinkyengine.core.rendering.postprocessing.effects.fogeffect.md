# FogEffect

Namespace: FrinkyEngine.Core.Rendering.PostProcessing.Effects

Distance-based fog effect that blends scene color toward a fog color based on depth.
 Supports linear, exponential, and exponential-squared falloff modes.

```csharp
public class FogEffect : FrinkyEngine.Core.Rendering.PostProcessing.PostProcessEffect, System.IDisposable
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [FObject](./frinkyengine.core.ecs.fobject) → [PostProcessEffect](./frinkyengine.core.rendering.postprocessing.postprocesseffect) → [FogEffect](./frinkyengine.core.rendering.postprocessing.effects.fogeffect)<br>
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

### **FogColor**

The fog color.

```csharp
public Color FogColor { get; set; }
```

#### Property Value

Color<br>

### **FogStart**

Distance at which linear fog starts (world units).

```csharp
public float FogStart { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **FogEnd**

Distance at which linear fog reaches full density (world units).

```csharp
public float FogEnd { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **Density**

Density factor for exponential fog modes.

```csharp
public float Density { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **Mode**

Fog falloff mode: Linear, Exponential, or ExponentialSquared.

```csharp
public FogMode Mode { get; set; }
```

#### Property Value

[FogMode](./frinkyengine.core.rendering.postprocessing.effects.fogmode)<br>

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

### **FogEffect()**

```csharp
public FogEffect()
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
