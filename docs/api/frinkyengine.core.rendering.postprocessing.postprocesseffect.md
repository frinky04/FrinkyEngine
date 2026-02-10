# PostProcessEffect

Namespace: FrinkyEngine.Core.Rendering.PostProcessing

Abstract base class for all post-processing effects.
 Subclass this to create custom effects — public read/write properties are auto-serialized and drawn in the inspector.

```csharp
public abstract class PostProcessEffect
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [PostProcessEffect](./frinkyengine.core.rendering.postprocessing.postprocesseffect)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **DisplayName**

Human-readable name shown in the editor UI.

```csharp
public abstract string DisplayName { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

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

### **PostProcessEffect()**

```csharp
protected PostProcessEffect()
```

## Methods

### **Initialize(String)**

Called once before the first render. Load shaders and other GPU resources here.

```csharp
public void Initialize(string shaderBasePath)
```

#### Parameters

`shaderBasePath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Directory containing engine post-processing shaders.

### **Render(Texture2D, RenderTexture2D, PostProcessContext)**

Renders this effect from `source` into `destination`.

```csharp
public abstract void Render(Texture2D source, RenderTexture2D destination, PostProcessContext context)
```

#### Parameters

`source` Texture2D<br>
The input color texture.

`destination` RenderTexture2D<br>
The render target to write to.

`context` [PostProcessContext](./frinkyengine.core.rendering.postprocessing.postprocesscontext)<br>
Per-frame context data (viewport size, depth texture, temp RT pool, etc.).

### **Dispose()**

Called when this effect is removed or the pipeline shuts down. Unload GPU resources here.

```csharp
public void Dispose()
```
