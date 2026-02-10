# AssetTagData

Namespace: FrinkyEngine.Core.Assets

Serialization container for tag definitions and per-asset tag assignments.

```csharp
public class AssetTagData
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [AssetTagData](./frinkyengine.core.assets.assettagdata)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **Tags**

All defined tags.

```csharp
public List<AssetTag> Tags { get; set; }
```

#### Property Value

[List&lt;AssetTag&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1)<br>

### **AssetTags**

Maps asset relative paths to lists of assigned tag names.

```csharp
public Dictionary<string, List<string>> AssetTags { get; set; }
```

#### Property Value

[Dictionary&lt;String, List&lt;String&gt;&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2)<br>

## Constructors

### **AssetTagData()**

```csharp
public AssetTagData()
```
