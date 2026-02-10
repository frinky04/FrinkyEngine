# AssetManager

Namespace: FrinkyEngine.Core.Assets

Singleton that loads and caches models and textures from the project's assets directory.

```csharp
public class AssetManager
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [AssetManager](./frinkyengine.core.assets.assetmanager)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **Instance**

The global asset manager instance.

```csharp
public static AssetManager Instance { get; }
```

#### Property Value

[AssetManager](./frinkyengine.core.assets.assetmanager)<br>

### **AssetsPath**

Root directory for resolving relative asset paths (defaults to "Assets").

```csharp
public string AssetsPath { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **EngineContentPath**

Root directory for resolving engine content asset paths (defaults to "EngineContent").

```csharp
public string EngineContentPath { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **ErrorTexture**

Fallback texture shown when a referenced texture file does not exist on disk.

```csharp
public Nullable<Texture2D> ErrorTexture { get; set; }
```

#### Property Value

[Nullable&lt;Texture2D&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **ErrorModel**

Fallback model shown when a referenced model file does not exist on disk.

```csharp
public Nullable<Model> ErrorModel { get; set; }
```

#### Property Value

[Nullable&lt;Model&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

## Constructors

### **AssetManager()**

```csharp
public AssetManager()
```

## Methods

### **ResolvePath(String)**

Combines a relative asset path with [AssetManager.AssetsPath](./frinkyengine.core.assets.assetmanager#assetspath) to produce a full file path.
 Paths with the `engine:` prefix are resolved against [AssetManager.EngineContentPath](./frinkyengine.core.assets.assetmanager#enginecontentpath) instead.

```csharp
public string ResolvePath(string relativePath)
```

#### Parameters

`relativePath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Path relative to the assets root.

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The resolved absolute path.

### **LoadModel(String)**

Loads a 3D model from the assets directory, returning a cached copy if already loaded.

```csharp
public Model LoadModel(string relativePath)
```

#### Parameters

`relativePath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Path relative to the assets root.

#### Returns

Model<br>
The loaded .

### **LoadTexture(String)**

Loads a texture from the assets directory, returning a cached copy if already loaded.

```csharp
public Texture2D LoadTexture(string relativePath)
```

#### Parameters

`relativePath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Path relative to the assets root.

#### Returns

Texture2D<br>
The loaded .

### **LoadAudioClip(String)**

Loads a short-form audio clip from the assets directory, returning a cached copy if already loaded.

```csharp
public Sound LoadAudioClip(string relativePath)
```

#### Parameters

`relativePath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Path relative to the assets root.

#### Returns

Sound<br>
The loaded .

### **LoadAudioStream(String)**

Loads a streamed audio asset from the assets directory, returning a cached stream if already loaded.

```csharp
public Music LoadAudioStream(string relativePath)
```

#### Parameters

`relativePath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Path relative to the assets root.

#### Returns

Music<br>
The loaded  stream.

### **GetTriplanarParamsTexture(Boolean, Single, Single, Boolean)**

Gets or creates a 1x1 float texture used to pass triplanar material parameters to shaders.

```csharp
public Texture2D GetTriplanarParamsTexture(bool enabled, float scale, float blendSharpness, bool useWorldSpace)
```

#### Parameters

`enabled` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
Whether triplanar mode is enabled for this material.

`scale` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Triplanar texture scale.

`blendSharpness` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Triplanar axis blend sharpness.

`useWorldSpace` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
Whether projection uses world-space coordinates.

#### Returns

Texture2D<br>
A cached 1x1 parameter texture.

### **InvalidateAsset(String)**

Removes a specific asset from the cache and unloads its GPU resources.

```csharp
public void InvalidateAsset(string relativePath)
```

#### Parameters

`relativePath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Path relative to the assets root (forward slashes are normalized).

### **UnloadAll()**

Unloads all cached models and textures, freeing GPU resources.

```csharp
public void UnloadAll()
```
