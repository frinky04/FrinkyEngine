# AssetReferenceUpdater

Namespace: FrinkyEngine.Core.Assets

Updates asset references when an asset is renamed, both on disk and in memory.

```csharp
public static class AssetReferenceUpdater
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [AssetReferenceUpdater](./frinkyengine.core.assets.assetreferenceupdater)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Methods

### **UpdateReferencesOnDisk(String, String, String)**

Scans all `.fscene` and `.fprefab` files under the project directory,
 replacing occurrences of the old asset path with the new one.
 Returns the number of files modified.

```csharp
public static int UpdateReferencesOnDisk(string assetsDirectory, string oldPath, string newPath)
```

#### Parameters

`assetsDirectory` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`oldPath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`newPath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

#### Returns

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **UpdateReferencesInScene(Scene, String, String)**

Updates all in-memory [AssetReference](./frinkyengine.core.assets.assetreference) properties on entities in the scene
 that match the old path, replacing them with the new path.
 Also updates `PrefabInstanceMetadata.AssetPath`.

```csharp
public static void UpdateReferencesInScene(Scene scene, string oldPath, string newPath)
```

#### Parameters

`scene` [Scene](./frinkyengine.core.scene.scene)<br>

`oldPath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`newPath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **FindReferencesOnDisk(String, String)**

Scans all `.fscene` and `.fprefab` files under the project directory,
 returning the relative paths of files that contain references to the given asset path.

```csharp
public static List<string> FindReferencesOnDisk(string assetsDirectory, string assetPath)
```

#### Parameters

`assetsDirectory` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`assetPath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

#### Returns

[List&lt;String&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1)<br>
