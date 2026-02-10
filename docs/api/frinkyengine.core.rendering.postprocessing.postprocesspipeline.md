# PostProcessPipeline

Namespace: FrinkyEngine.Core.Rendering.PostProcessing

Manages the post-processing render pipeline: ping-pong render targets, depth pre-pass,
 temporary RT pool, and the per-frame effect execution loop.

```csharp
public class PostProcessPipeline
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [PostProcessPipeline](./frinkyengine.core.rendering.postprocessing.postprocesspipeline)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Constructors

### **PostProcessPipeline()**

```csharp
public PostProcessPipeline()
```

## Methods

### **Initialize(String)**

Initializes the pipeline, loading the depth pre-pass shader from the given directory.
 Safe to call every frame — only loads resources on the first call.

```csharp
public void Initialize(string shaderBasePath)
```

#### Parameters

`shaderBasePath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Directory containing engine shaders (e.g. "Shaders").

### **EnsureResources(Int32, Int32)**

Ensures ping-pong and depth render textures match the current viewport size.

```csharp
public void EnsureResources(int width, int height)
```

#### Parameters

`width` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>
Viewport width.

`height` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>
Viewport height.

### **Execute(PostProcessStackComponent, Texture2D, Camera3D, CameraComponent, SceneRenderer, Scene, Int32, Int32, Boolean, Texture2D)**

Executes the post-processing stack on the given scene color texture.
 Returns the final post-processed texture, or the original if no effects run.

```csharp
public Texture2D Execute(PostProcessStackComponent stack, Texture2D sceneColor, Camera3D camera, CameraComponent cam, SceneRenderer sceneRenderer, Scene scene, int width, int height, bool isEditorMode, Texture2D sceneDepthTexture)
```

#### Parameters

`stack` [PostProcessStackComponent](./frinkyengine.core.components.postprocessstackcomponent)<br>
The post-process stack component on the camera.

`sceneColor` Texture2D<br>
The rendered scene color texture.

`camera` Camera3D<br>
The camera used for rendering.

`cam` [CameraComponent](./frinkyengine.core.components.cameracomponent)<br>
Optional camera component for near/far plane data.

`sceneRenderer` [SceneRenderer](./frinkyengine.core.rendering.scenerenderer)<br>
Scene renderer for depth pre-pass.

`scene` [Scene](./frinkyengine.core.scene.scene)<br>
The current scene.

`width` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>
Viewport width.

`height` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>
Viewport height.

`isEditorMode` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
Whether we're in editor mode.

`sceneDepthTexture` Texture2D<br>
Optional hardware depth texture from the scene RT. When provided, depth is linearized via a fullscreen blit instead of a geometry re-render.

#### Returns

Texture2D<br>
The final output texture.

### **Shutdown()**

Shuts down the pipeline, releasing all GPU resources.

```csharp
public void Shutdown()
```

### **LoadRenderTextureWithDepthTexture(Int32, Int32)**

Creates a render texture whose depth attachment is a sampleable texture instead of
 a renderbuffer. This allows post-processing effects to read the scene depth directly
 via a fullscreen linearize blit, eliminating the need for a geometry depth pre-pass.

```csharp
public static RenderTexture2D LoadRenderTextureWithDepthTexture(int width, int height)
```

#### Parameters

`width` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>
Texture width.

`height` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>
Texture height.

#### Returns

RenderTexture2D<br>
A render texture with a sampleable depth texture attachment.
