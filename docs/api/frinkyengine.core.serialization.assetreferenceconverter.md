# AssetReferenceConverter

Namespace: FrinkyEngine.Core.Serialization

Serializes [AssetReference](./frinkyengine.core.assets.assetreference) as a plain JSON string (just the path).
 Provides full backward compatibility with existing string-based asset paths.

```csharp
public class AssetReferenceConverter : System.Text.Json.Serialization.JsonConverter`1[[FrinkyEngine.Core.Assets.AssetReference, FrinkyEngine.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → JsonConverter → JsonConverter&lt;AssetReference&gt; → [AssetReferenceConverter](./frinkyengine.core.serialization.assetreferenceconverter)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **HandleNull**

```csharp
public bool HandleNull { get; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Type**

```csharp
public Type Type { get; }
```

#### Property Value

[Type](https://docs.microsoft.com/en-us/dotnet/api/system.type)<br>

## Constructors

### **AssetReferenceConverter()**

```csharp
public AssetReferenceConverter()
```

## Methods

### **Read(Utf8JsonReader&, Type, JsonSerializerOptions)**

```csharp
public AssetReference Read(Utf8JsonReader& reader, Type typeToConvert, JsonSerializerOptions options)
```

#### Parameters

`reader` Utf8JsonReader&<br>

`typeToConvert` [Type](https://docs.microsoft.com/en-us/dotnet/api/system.type)<br>

`options` JsonSerializerOptions<br>

#### Returns

[AssetReference](./frinkyengine.core.assets.assetreference)<br>

### **Write(Utf8JsonWriter, AssetReference, JsonSerializerOptions)**

```csharp
public void Write(Utf8JsonWriter writer, AssetReference value, JsonSerializerOptions options)
```

#### Parameters

`writer` Utf8JsonWriter<br>

`value` [AssetReference](./frinkyengine.core.assets.assetreference)<br>

`options` JsonSerializerOptions<br>
