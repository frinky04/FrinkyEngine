# AssetReference

Namespace: FrinkyEngine.Core.Assets

A reference to a project asset by its relative path.

```csharp
public struct AssetReference
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [AssetReference](./frinkyengine.core.assets.assetreference)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Fields

### **EnginePrefix**

Prefix used in serialized paths to denote engine-provided assets.

```csharp
public static string EnginePrefix;
```

## Properties

### **Path**

The asset-relative path to the referenced file.

```csharp
public string Path { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **IsEmpty**

True when no asset is referenced.

```csharp
public bool IsEmpty { get; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **IsEngineAsset**

True when this reference points to an engine-provided asset.

```csharp
public bool IsEngineAsset { get; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

## Constructors

### **AssetReference(String)**

Creates a new asset reference with the given path.

```csharp
AssetReference(string path)
```

#### Parameters

`path` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

## Methods

### **HasEnginePrefix(String)**

Returns true if the given path starts with the engine prefix.

```csharp
bool HasEnginePrefix(string path)
```

#### Parameters

`path` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **StripEnginePrefix(String)**

Strips the engine prefix from a path. Returns the path unchanged if no prefix is present.

```csharp
string StripEnginePrefix(string path)
```

#### Parameters

`path` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **ToString()**

```csharp
string ToString()
```

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
