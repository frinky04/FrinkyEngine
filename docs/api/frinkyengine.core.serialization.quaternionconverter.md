# QuaternionConverter

Namespace: FrinkyEngine.Core.Serialization

JSON converter that serializes [Quaternion](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.quaternion) as `{ "x": ..., "y": ..., "z": ..., "w": ... }`.

```csharp
public class QuaternionConverter : System.Text.Json.Serialization.JsonConverter`1[[System.Numerics.Quaternion, System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → JsonConverter → JsonConverter&lt;Quaternion&gt; → [QuaternionConverter](./frinkyengine.core.serialization.quaternionconverter)<br>
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

### **QuaternionConverter()**

```csharp
public QuaternionConverter()
```

## Methods

### **Read(Utf8JsonReader&, Type, JsonSerializerOptions)**

```csharp
public Quaternion Read(Utf8JsonReader& reader, Type typeToConvert, JsonSerializerOptions options)
```

#### Parameters

`reader` Utf8JsonReader&<br>

`typeToConvert` [Type](https://docs.microsoft.com/en-us/dotnet/api/system.type)<br>

`options` JsonSerializerOptions<br>

#### Returns

[Quaternion](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.quaternion)<br>

### **Write(Utf8JsonWriter, Quaternion, JsonSerializerOptions)**

```csharp
public void Write(Utf8JsonWriter writer, Quaternion value, JsonSerializerOptions options)
```

#### Parameters

`writer` Utf8JsonWriter<br>

`value` [Quaternion](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.quaternion)<br>

`options` JsonSerializerOptions<br>
