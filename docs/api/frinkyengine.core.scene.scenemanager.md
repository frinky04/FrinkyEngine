# SceneManager

Namespace: FrinkyEngine.Core.Scene

Singleton that owns the active [Scene](./frinkyengine.core.scene.scene) and provides load/save operations.

```csharp
public class SceneManager
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [SceneManager](./frinkyengine.core.scene.scenemanager)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **Instance**

The global scene manager instance.

```csharp
public static SceneManager Instance { get; }
```

#### Property Value

[SceneManager](./frinkyengine.core.scene.scenemanager)<br>

### **ActiveScene**

The currently loaded scene, or `null` if no scene is active.

```csharp
public Scene ActiveScene { get; private set; }
```

#### Property Value

[Scene](./frinkyengine.core.scene.scene)<br>

### **IsSaveDisabled**

When true, saving the scene is prohibited (e.g., during Play mode).
 The Editor sets this when entering/exiting Play mode.

```csharp
public bool IsSaveDisabled { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

## Constructors

### **SceneManager()**

```csharp
public SceneManager()
```

## Methods

### **NewScene(String)**

Creates a new empty scene and makes it the active scene.

```csharp
public Scene NewScene(string name)
```

#### Parameters

`name` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Display name for the new scene.

#### Returns

[Scene](./frinkyengine.core.scene.scene)<br>
The newly created scene.

### **SetActiveScene(Scene)**

Sets the given scene as the active scene without saving or unloading the previous one.

```csharp
public void SetActiveScene(Scene scene)
```

#### Parameters

`scene` [Scene](./frinkyengine.core.scene.scene)<br>
The scene to activate.

### **SaveScene(String)**

Saves the active scene to the specified file path in `.fscene` JSON format.

```csharp
public void SaveScene(string path)
```

#### Parameters

`path` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Destination file path.

#### Exceptions

[InvalidOperationException](https://docs.microsoft.com/en-us/dotnet/api/system.invalidoperationexception)<br>
Thrown when [SceneManager.IsSaveDisabled](./frinkyengine.core.scene.scenemanager#issavedisabled) is true.

### **LoadSceneByName(String)**

Resolves a scene name or path via [AssetDatabase](./frinkyengine.core.assets.assetdatabase) and loads the scene.
 Accepts a bare name (e.g. "Level"), a name with extension ("Level.fscene"),
 or a full/relative file path as a fallback.

```csharp
public Scene LoadSceneByName(string nameOrPath)
```

#### Parameters

`nameOrPath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Scene name, relative asset path, or absolute file path.

#### Returns

[Scene](./frinkyengine.core.scene.scene)<br>
The loaded scene, or `null` if resolution or loading failed.

### **LoadScene(String)**

Loads a scene from the specified `.fscene` file and makes it the active scene.

```csharp
public Scene LoadScene(string path)
```

#### Parameters

`path` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Path to the scene file.

#### Returns

[Scene](./frinkyengine.core.scene.scene)<br>
The loaded scene, or `null` if loading failed.
