# AssetDatabase

Namespace: FrinkyEngine.Core.Assets

Singleton that scans a project's assets directory and provides filtered access to discovered files.

```csharp
public class AssetDatabase
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [AssetDatabase](./frinkyengine.core.assets.assetdatabase)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **Instance**

The global asset database instance.

```csharp
public static AssetDatabase Instance { get; }
```

#### Property Value

[AssetDatabase](./frinkyengine.core.assets.assetdatabase)<br>

### **EngineContentPath**

The absolute path to the engine content directory, or empty if not scanned.

```csharp
public string EngineContentPath { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

## Constructors

### **AssetDatabase()**

```csharp
public AssetDatabase()
```

## Methods

### **RegisterExtension(String, AssetType)**

Registers a custom file extension to be recognized as a specific asset type during scanning.

```csharp
public void RegisterExtension(string ext, AssetType type)
```

#### Parameters

`ext` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
File extension (with or without leading dot).

`type` [AssetType](./frinkyengine.core.assets.assettype)<br>
The asset type to associate with the extension.

### **Scan(String)**

Scans the given directory recursively and rebuilds the asset list.

```csharp
public void Scan(string assetsPath)
```

#### Parameters

`assetsPath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Absolute path to the assets root directory.

### **ScanEngineContent(String)**

Scans the engine content directory and rebuilds the engine asset list.

```csharp
public void ScanEngineContent(string engineContentPath)
```

#### Parameters

`engineContentPath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Absolute path to the engine content root directory.

### **GetEngineAssets(Nullable&lt;AssetType&gt;)**

Gets all discovered engine assets, optionally filtered by type.

```csharp
public IReadOnlyList<AssetEntry> GetEngineAssets(Nullable<AssetType> filter)
```

#### Parameters

`filter` [Nullable&lt;AssetType&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>
If specified, only assets of this type are returned.

#### Returns

[IReadOnlyList&lt;AssetEntry&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1)<br>
A read-only list of matching engine asset entries.

### **Refresh()**

Rescans the previously scanned assets directory.

```csharp
public void Refresh()
```

### **GetAssets(Nullable&lt;AssetType&gt;)**

Gets all discovered assets, optionally filtered by type.

```csharp
public IReadOnlyList<AssetEntry> GetAssets(Nullable<AssetType> filter)
```

#### Parameters

`filter` [Nullable&lt;AssetType&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>
If specified, only assets of this type are returned.

#### Returns

[IReadOnlyList&lt;AssetEntry&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1)<br>
A read-only list of matching asset entries.

### **GetAssetsInDirectory(String, Nullable&lt;AssetType&gt;)**

Gets assets that are direct children of the specified directory (non-recursive).

```csharp
public IReadOnlyList<AssetEntry> GetAssetsInDirectory(string relativeDir, Nullable<AssetType> filter)
```

#### Parameters

`relativeDir` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Relative directory path (empty string for root).

`filter` [Nullable&lt;AssetType&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>
If specified, only assets of this type are returned.

#### Returns

[IReadOnlyList&lt;AssetEntry&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1)<br>
A read-only list of matching asset entries.

### **GetSubdirectories(String)**

Gets the names of immediate subdirectories under the specified directory.

```csharp
public IReadOnlyList<string> GetSubdirectories(string relativeDir)
```

#### Parameters

`relativeDir` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Relative directory path (empty string for root).

#### Returns

[IReadOnlyList&lt;String&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1)<br>
An alphabetically sorted list of subdirectory names.

### **AssetExists(String)**

Returns true if an asset with the given relative path exists in the database.
 Paths with the `engine:` prefix are checked against the engine content index.

```csharp
public bool AssetExists(string relativePath)
```

#### Parameters

`relativePath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Asset-relative path to check.

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
True if the asset exists.

### **ResolveAssetPath(String)**

Resolves a filename or relative path to a full relative path.
 Bare filenames are resolved via the filename index; paths containing separators use the path index directly.
 Paths with the `engine:` prefix are resolved against the engine content index and returned with the prefix intact.

```csharp
public string ResolveAssetPath(string nameOrPath)
```

#### Parameters

`nameOrPath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
A bare filename (e.g. "player.glb") or relative path (e.g. "Models/player.glb").

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The full relative path if unambiguously resolved, or null.

### **IsFileNameUnique(String)**

Returns true if the given filename maps to exactly one asset in the database.

```csharp
public bool IsFileNameUnique(string fileName)
```

#### Parameters

`fileName` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The bare filename to check.

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
True if there is exactly one asset with that filename.

### **IsEngineFileNameUnique(String)**

Returns true if the given filename maps to exactly one engine asset in the database.

```csharp
public bool IsEngineFileNameUnique(string fileName)
```

#### Parameters

`fileName` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The bare filename to check.

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
True if there is exactly one engine asset with that filename.

### **GetCanonicalName(String)**

Returns the shortest unambiguous name for an asset: just the filename if unique, or the full relative path if ambiguous.

```csharp
public string GetCanonicalName(string relativePath)
```

#### Parameters

`relativePath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The full relative path of the asset.

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The canonical name to store in references.

### **GetEngineCanonicalName(String)**

Returns the shortest unambiguous name for an engine asset, prefixed with `engine:`.

```csharp
public string GetEngineCanonicalName(string relativePath)
```

#### Parameters

`relativePath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The engine-relative path of the asset (without `engine:` prefix).

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The canonical name with `engine:` prefix.

### **AssetExistsByName(String)**

Returns true if a filename or relative path can be resolved to an existing asset.

```csharp
public bool AssetExistsByName(string nameOrPath)
```

#### Parameters

`nameOrPath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
A bare filename or relative path.

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
True if the asset can be resolved.

### **Clear()**

Clears all cached asset entries and resets the scan path.

```csharp
public void Clear()
```
