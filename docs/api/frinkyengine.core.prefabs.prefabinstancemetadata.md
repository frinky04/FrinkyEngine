# PrefabInstanceMetadata

Namespace: FrinkyEngine.Core.Prefabs

```csharp
public class PrefabInstanceMetadata
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [PrefabInstanceMetadata](./frinkyengine.core.prefabs.prefabinstancemetadata)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **IsRoot**

```csharp
public bool IsRoot { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **AssetPath**

```csharp
public AssetReference AssetPath { get; set; }
```

#### Property Value

[AssetReference](./frinkyengine.core.assets.assetreference)<br>

### **SourceNodeId**

```csharp
public string SourceNodeId { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **Overrides**

```csharp
public PrefabOverridesData Overrides { get; set; }
```

#### Property Value

[PrefabOverridesData](./frinkyengine.core.prefabs.prefaboverridesdata)<br>

## Constructors

### **PrefabInstanceMetadata()**

```csharp
public PrefabInstanceMetadata()
```

## Methods

### **Clone()**

```csharp
public PrefabInstanceMetadata Clone()
```

#### Returns

[PrefabInstanceMetadata](./frinkyengine.core.prefabs.prefabinstancemetadata)<br>
