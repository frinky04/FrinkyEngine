# AssetEntry

Namespace: FrinkyEngine.Core.Assets

Represents a single asset file discovered in the project's assets directory.

```csharp
public class AssetEntry
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [AssetEntry](./frinkyengine.core.assets.assetentry)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **RelativePath**

Path relative to the assets root, using forward slashes.

```csharp
public string RelativePath { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **FileName**

File name with extension (e.g. "player.glb").

```csharp
public string FileName { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **Extension**

Lowercase file extension including the dot (e.g. ".png").

```csharp
public string Extension { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **Type**

The detected asset type based on file extension.

```csharp
public AssetType Type { get; }
```

#### Property Value

[AssetType](./frinkyengine.core.assets.assettype)<br>

### **IsEngineAsset**

True if this asset comes from the engine content directory rather than the project.

```csharp
public bool IsEngineAsset { get; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

## Constructors

### **AssetEntry(String, AssetType, Boolean)**

Creates a new asset entry.

```csharp
public AssetEntry(string relativePath, AssetType type, bool isEngineAsset)
```

#### Parameters

`relativePath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Path relative to the assets root.

`type` [AssetType](./frinkyengine.core.assets.assettype)<br>
The asset type classification.

`isEngineAsset` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
Whether this asset is from engine content.
