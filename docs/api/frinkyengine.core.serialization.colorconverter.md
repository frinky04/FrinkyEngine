# ColorConverter

Namespace: FrinkyEngine.Core.Serialization

JSON converter that serializes  as `{ "r": ..., "g": ..., "b": ..., "a": ... }`.

```csharp
public class ColorConverter : System.Text.Json.Serialization.JsonConverter`1[[Raylib_cs.Color, Raylib-cs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → JsonConverter → JsonConverter&lt;Color&gt; → [ColorConverter](./frinkyengine.core.serialization.colorconverter)<br>
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

### **ColorConverter()**

```csharp
public ColorConverter()
```

## Methods

### **Read(Utf8JsonReader&, Type, JsonSerializerOptions)**

```csharp
public Color Read(Utf8JsonReader& reader, Type typeToConvert, JsonSerializerOptions options)
```

#### Parameters

`reader` Utf8JsonReader&<br>

`typeToConvert` [Type](https://docs.microsoft.com/en-us/dotnet/api/system.type)<br>

`options` JsonSerializerOptions<br>

#### Returns

Color<br>

### **Write(Utf8JsonWriter, Color, JsonSerializerOptions)**

```csharp
public void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
```

#### Parameters

`writer` Utf8JsonWriter<br>

`value` Color<br>

`options` JsonSerializerOptions<br>
