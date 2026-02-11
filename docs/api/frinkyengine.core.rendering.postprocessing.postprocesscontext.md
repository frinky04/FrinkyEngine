# PostProcessContext

Namespace: FrinkyEngine.Core.Rendering.PostProcessing

Per-frame data passed to post-processing effects during rendering.
 Provides viewport dimensions, depth texture access, camera parameters, and temporary RT allocation.

```csharp
public class PostProcessContext
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [PostProcessContext](./frinkyengine.core.rendering.postprocessing.postprocesscontext)

## Properties

### **Width**

Current viewport width in pixels.

```csharp
public int Width { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **Height**

Current viewport height in pixels.

```csharp
public int Height { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **DepthTexture**

Linear depth texture (R channel = normalized linear depth 0..1). Only valid when an effect requests depth.

```csharp
public Texture2D DepthTexture { get; set; }
```

#### Property Value

Texture2D<br>

### **NearPlane**

Camera near plane distance.

```csharp
public float NearPlane { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **FarPlane**

Camera far plane distance.

```csharp
public float FarPlane { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **CameraPosition**

World-space camera position.

```csharp
public Vector3 CameraPosition { get; set; }
```

#### Property Value

[Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

### **FieldOfViewDegrees**

Camera vertical field of view in degrees.

```csharp
public float FieldOfViewDegrees { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **AspectRatio**

Viewport aspect ratio (width / height).

```csharp
public float AspectRatio { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

## Constructors

### **PostProcessContext()**

```csharp
public PostProcessContext()
```

## Methods

### **GetTemporaryRT(Int32, Int32)**

Allocates a temporary render texture from the pool. Released automatically after each effect.
 Reuses pooled RTs of the same size to avoid per-frame GPU framebuffer allocation.

```csharp
public RenderTexture2D GetTemporaryRT(int width, int height)
```

#### Parameters

`width` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>
Texture width (defaults to viewport width).

`height` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>
Texture height (defaults to viewport height).

#### Returns

RenderTexture2D<br>
A temporary render texture.

### **ReleaseTemporaryRTs()**

Returns all temporary render textures allocated during the current effect back to the pool.
 Called automatically by the pipeline after each effect's [PostProcessEffect.Render(Texture2D, RenderTexture2D, PostProcessContext)](./frinkyengine.core.rendering.postprocessing.postprocesseffect#rendertexture2d-rendertexture2d-postprocesscontext).

```csharp
public void ReleaseTemporaryRTs()
```

### **DisposePool()**

Unloads all pooled render textures, freeing GPU resources.
 Called on viewport resize and pipeline shutdown.

```csharp
public void DisposePool()
```

### **Blit(Texture2D, RenderTexture2D, Nullable&lt;Shader&gt;)**

Performs a fullscreen blit from `source` into `dest`,
 optionally applying `shader`.

```csharp
public static void Blit(Texture2D source, RenderTexture2D dest, Nullable<Shader> shader)
```

#### Parameters

`source` Texture2D<br>
Source color texture.

`dest` RenderTexture2D<br>
Destination render texture.

`shader` [Nullable&lt;Shader&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>
Optional shader to apply. Pass `null` for a simple copy.
