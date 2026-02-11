# AssetTagDatabase

Namespace: FrinkyEngine.Core.Assets

Manages asset tag definitions and per-asset tag assignments, persisted as JSON.

```csharp
public class AssetTagDatabase
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [AssetTagDatabase](./frinkyengine.core.assets.assettagdatabase)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Fields

### **FileName**

Default file name for the tag database.

```csharp
public static string FileName;
```

## Constructors

### **AssetTagDatabase()**

```csharp
public AssetTagDatabase()
```

## Methods

### **LoadOrCreate(String)**

Loads the tag database from the project directory, or creates a new empty one.

```csharp
public static AssetTagDatabase LoadOrCreate(string projectDirectory)
```

#### Parameters

`projectDirectory` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

#### Returns

[AssetTagDatabase](./frinkyengine.core.assets.assettagdatabase)<br>

### **Save(String)**

Saves the tag database to the project directory, sorting entries for clean diffs.

```csharp
public void Save(string projectDirectory)
```

#### Parameters

`projectDirectory` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **GetAllTags()**

Returns all defined tags.

```csharp
public List<AssetTag> GetAllTags()
```

#### Returns

[List&lt;AssetTag&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1)<br>

### **GetTagsForAsset(String)**

Returns the resolved tag objects assigned to the given asset.

```csharp
public List<AssetTag> GetTagsForAsset(string relativePath)
```

#### Parameters

`relativePath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

#### Returns

[List&lt;AssetTag&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1)<br>

### **SetAssetTags(String, List&lt;String&gt;)**

Replaces all tag assignments for an asset.

```csharp
public void SetAssetTags(string relativePath, List<string> tagNames)
```

#### Parameters

`relativePath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`tagNames` [List&lt;String&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1)<br>

### **AddTagToAssets(IEnumerable&lt;String&gt;, String)**

Adds a tag to multiple assets at once.

```csharp
public void AddTagToAssets(IEnumerable<string> relativePaths, string tagName)
```

#### Parameters

`relativePaths` [IEnumerable&lt;String&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1)<br>

`tagName` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **RemoveTagFromAssets(IEnumerable&lt;String&gt;, String)**

Removes a tag from multiple assets at once.

```csharp
public void RemoveTagFromAssets(IEnumerable<string> relativePaths, string tagName)
```

#### Parameters

`relativePaths` [IEnumerable&lt;String&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1)<br>

`tagName` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **CreateTag(String, String)**

Creates a new tag definition if one with the same name does not already exist.

```csharp
public void CreateTag(string name, string hexColor)
```

#### Parameters

`name` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`hexColor` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **DeleteTag(String)**

Deletes a tag definition and removes it from all asset assignments.

```csharp
public void DeleteTag(string name)
```

#### Parameters

`name` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **RenameTag(String, String)**

Renames a tag and updates all asset assignments to use the new name.

```csharp
public void RenameTag(string oldName, string newName)
```

#### Parameters

`oldName` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`newName` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **UpdateTagColor(String, String)**

Updates the display color for a tag.

```csharp
public void UpdateTagColor(string name, string hexColor)
```

#### Parameters

`name` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`hexColor` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **GetAssetsWithTag(String)**

Returns the set of asset paths that have the given tag assigned.

```csharp
public HashSet<string> GetAssetsWithTag(string tagName)
```

#### Parameters

`tagName` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

#### Returns

[HashSet&lt;String&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1)<br>

### **AssetHasTag(String, String)**

Checks whether the given asset has a specific tag assigned.

```csharp
public bool AssetHasTag(string relativePath, string tagName)
```

#### Parameters

`relativePath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`tagName` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **RemoveAssetPath(String)**

Removes all tag assignments for a deleted asset.

```csharp
public void RemoveAssetPath(string relativePath)
```

#### Parameters

`relativePath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **RenameAssetPath(String, String)**

Updates the key for an asset's tag assignments when the asset is renamed.

```csharp
public void RenameAssetPath(string oldPath, string newPath)
```

#### Parameters

`oldPath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`newPath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **CleanupStaleEntries(IReadOnlySet&lt;String&gt;)**

Removes tag assignments for assets that no longer exist.

```csharp
public void CleanupStaleEntries(IReadOnlySet<string> existingAssetPaths)
```

#### Parameters

`existingAssetPaths` [IReadOnlySet&lt;String&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlyset-1)<br>
