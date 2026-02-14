# SceneRenderer

Namespace: FrinkyEngine.Core.Rendering

Renders scenes using a Forward+ tiled lighting pipeline with support for multiple light types.

```csharp
public class SceneRenderer
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [SceneRenderer](./frinkyengine.core.rendering.scenerenderer)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **LastFrameDrawCallCount**

Number of draw calls issued in the most recent [SceneRenderer.Render(Scene, Camera3D, Nullable&lt;RenderTexture2D&gt;, Action, Boolean)](./frinkyengine.core.rendering.scenerenderer#renderscene-camera3d-nullablerendertexture2d-action-boolean) pass.

```csharp
public int LastFrameDrawCallCount { get; private set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **LastFrameSkinnedMeshCount**

Number of skinned meshes that had GPU skinning prepared in the most recent [SceneRenderer.Render(Scene, Camera3D, Nullable&lt;RenderTexture2D&gt;, Action, Boolean)](./frinkyengine.core.rendering.scenerenderer#renderscene-camera3d-nullablerendertexture2d-action-boolean) pass.

```csharp
public int LastFrameSkinnedMeshCount { get; private set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

## Constructors

### **SceneRenderer()**

```csharp
public SceneRenderer()
```

## Methods

### **GetForwardPlusFrameStats()**

Gets diagnostic statistics from the most recent Forward+ render pass.

```csharp
public ForwardPlusFrameStats GetForwardPlusFrameStats()
```

#### Returns

[ForwardPlusFrameStats](./frinkyengine.core.rendering.scenerenderer.forwardplusframestats)<br>
A snapshot of the current frame's lighting statistics.

### **GetAutoInstancingFrameStats()**

Gets automatic instancing diagnostics from the most recent [SceneRenderer.Render(Scene, Camera3D, Nullable&lt;RenderTexture2D&gt;, Action, Boolean)](./frinkyengine.core.rendering.scenerenderer#renderscene-camera3d-nullablerendertexture2d-action-boolean) pass.

```csharp
public AutoInstancingFrameStats GetAutoInstancingFrameStats()
```

#### Returns

[AutoInstancingFrameStats](./frinkyengine.core.rendering.scenerenderer.autoinstancingframestats)<br>
A snapshot of batching and instanced submission counts for the frame.

### **ConfigureForwardPlus(ForwardPlusSettings)**

Applies new Forward+ configuration settings, reallocating tile buffers if needed.

```csharp
public void ConfigureForwardPlus(ForwardPlusSettings settings)
```

#### Parameters

`settings` [ForwardPlusSettings](./frinkyengine.core.rendering.forwardplussettings)<br>
The new settings to apply (values will be normalized/clamped).

### **LoadShader(String, String)**

Loads the lighting shader from vertex and fragment shader files.

```csharp
public void LoadShader(string vsPath, string fsPath)
```

#### Parameters

`vsPath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Path to the vertex shader file.

`fsPath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Path to the fragment shader file.

### **UnloadShader()**

Unloads all shaders and releases Forward+ GPU textures.

```csharp
public void UnloadShader()
```

### **Render(Scene, Camera3D, Nullable&lt;RenderTexture2D&gt;, Action, Boolean)**

Renders the scene from the given camera, optionally into a render texture.

```csharp
public void Render(Scene scene, Camera3D camera, Nullable<RenderTexture2D> renderTarget, Action postSceneRender, bool isEditorMode)
```

#### Parameters

`scene` [Scene](./frinkyengine.core.scene.scene)<br>
The scene to render.

`camera` Camera3D<br>
The camera viewpoint.

`renderTarget` [Nullable&lt;RenderTexture2D&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>
Optional render texture target (renders to screen if `null`).

`postSceneRender` [Action](https://docs.microsoft.com/en-us/dotnet/api/system.action)<br>
Optional callback invoked after 3D drawing but before EndMode3D.

`isEditorMode` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
When `true`, editor-only objects and the grid are drawn.

### **RenderDepthPrePass(Scene, Camera3D, RenderTexture2D, Shader, Boolean)**

Renders scene geometry into a depth-only render texture using the provided depth shader.
 The output stores normalized linear depth in the R channel.
 Shader uniforms (nearPlane, farPlane) must be set by the caller before invoking this method.

```csharp
public void RenderDepthPrePass(Scene scene, Camera3D camera, RenderTexture2D depthTarget, Shader depthShader, bool isEditorMode)
```

#### Parameters

`scene` [Scene](./frinkyengine.core.scene.scene)<br>
The scene to render.

`camera` Camera3D<br>
The camera viewpoint.

`depthTarget` RenderTexture2D<br>
The render texture to write depth into.

`depthShader` Shader<br>
The depth pre-pass shader (uniforms already configured).

`isEditorMode` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
When `true`, editor-only objects participate.

### **RenderSelectionMask(Scene, Camera3D, IReadOnlyList&lt;Entity&gt;, RenderTexture2D, Boolean)**

Renders a binary selection mask of the specified entities into a render texture, used for outline effects.

```csharp
public void RenderSelectionMask(Scene scene, Camera3D camera, IReadOnlyList<Entity> selectedEntities, RenderTexture2D renderTarget, bool isEditorMode)
```

#### Parameters

`scene` [Scene](./frinkyengine.core.scene.scene)<br>
The scene containing the entities.

`camera` Camera3D<br>
The camera viewpoint.

`selectedEntities` [IReadOnlyList&lt;Entity&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1)<br>
The entities to highlight.

`renderTarget` RenderTexture2D<br>
The render texture to draw the mask into.

`isEditorMode` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
When `true`, editor-only objects participate in depth testing.
