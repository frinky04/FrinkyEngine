# PrefabDatabase

Namespace: FrinkyEngine.Core.Assets

```csharp
public class PrefabDatabase
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [PrefabDatabase](./frinkyengine.core.assets.prefabdatabase)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **Instance**

```csharp
public static PrefabDatabase Instance { get; }
```

#### Property Value

[PrefabDatabase](./frinkyengine.core.assets.prefabdatabase)<br>

## Constructors

### **PrefabDatabase()**

```csharp
public PrefabDatabase()
```

## Methods

### **Load(String, Boolean)**

```csharp
public PrefabAssetData Load(string relativePath, bool resolveVariants)
```

#### Parameters

`relativePath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`resolveVariants` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

#### Returns

[PrefabAssetData](./frinkyengine.core.prefabs.prefabassetdata)<br>

### **Save(String, PrefabAssetData)**

```csharp
public bool Save(string relativePath, PrefabAssetData prefab)
```

#### Parameters

`relativePath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`prefab` [PrefabAssetData](./frinkyengine.core.prefabs.prefabassetdata)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Invalidate(String)**

```csharp
public void Invalidate(string relativePath)
```

#### Parameters

`relativePath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **Clear()**

```csharp
public void Clear()
```
