# Vector3Converter

Namespace: FrinkyEngine.Core.Serialization

JSON converter that serializes [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3) as `{ "x": ..., "y": ..., "z": ... }`.

```csharp
public class Vector3Converter : System.Text.Json.Serialization.JsonConverter`1[[System.Numerics.Vector3, System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → JsonConverter → JsonConverter&lt;Vector3&gt; → [Vector3Converter](./frinkyengine.core.serialization.vector3converter)<br>
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

### **Vector3Converter()**

```csharp
public Vector3Converter()
```

## Methods

### **Read(Utf8JsonReader&, Type, JsonSerializerOptions)**

```csharp
public Vector3 Read(Utf8JsonReader& reader, Type typeToConvert, JsonSerializerOptions options)
```

#### Parameters

`reader` Utf8JsonReader&<br>

`typeToConvert` [Type](https://docs.microsoft.com/en-us/dotnet/api/system.type)<br>

`options` JsonSerializerOptions<br>

#### Returns

[Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

### **Write(Utf8JsonWriter, Vector3, JsonSerializerOptions)**

```csharp
public void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
```

#### Parameters

`writer` Utf8JsonWriter<br>

`value` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

`options` JsonSerializerOptions<br>
