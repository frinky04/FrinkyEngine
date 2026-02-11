# PrefabAssetData

Namespace: FrinkyEngine.Core.Prefabs

```csharp
public class PrefabAssetData
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [PrefabAssetData](./frinkyengine.core.prefabs.prefabassetdata)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **Name**

```csharp
public string Name { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **SourcePrefab**

```csharp
public AssetReference SourcePrefab { get; set; }
```

#### Property Value

[AssetReference](./frinkyengine.core.assets.assetreference)<br>

### **Root**

```csharp
public PrefabNodeData Root { get; set; }
```

#### Property Value

[PrefabNodeData](./frinkyengine.core.prefabs.prefabnodedata)<br>

### **VariantOverrides**

```csharp
public PrefabOverridesData VariantOverrides { get; set; }
```

#### Property Value

[PrefabOverridesData](./frinkyengine.core.prefabs.prefaboverridesdata)<br>

## Constructors

### **PrefabAssetData()**

```csharp
public PrefabAssetData()
```

## Methods

### **Clone()**

```csharp
public PrefabAssetData Clone()
```

#### Returns

[PrefabAssetData](./frinkyengine.core.prefabs.prefabassetdata)<br>
