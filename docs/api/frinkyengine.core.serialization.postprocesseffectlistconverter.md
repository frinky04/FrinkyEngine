# PostProcessEffectListConverter

Namespace: FrinkyEngine.Core.Serialization

JSON converter for [List&lt;T&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1) that serializes each effect
 with a `$type` discriminator and its public read/write properties.

```csharp
public class PostProcessEffectListConverter : System.Text.Json.Serialization.JsonConverter`1[[System.Collections.Generic.List`1[[FrinkyEngine.Core.Rendering.PostProcessing.PostProcessEffect, FrinkyEngine.Core, Version=0.5.4.0, Culture=neutral, PublicKeyToken=null]], System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → JsonConverter → JsonConverter&lt;List&lt;PostProcessEffect&gt;&gt; → [PostProcessEffectListConverter](./frinkyengine.core.serialization.postprocesseffectlistconverter)<br>
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

### **PostProcessEffectListConverter()**

```csharp
public PostProcessEffectListConverter()
```

## Methods

### **Read(Utf8JsonReader&, Type, JsonSerializerOptions)**

```csharp
public List<PostProcessEffect> Read(Utf8JsonReader& reader, Type typeToConvert, JsonSerializerOptions options)
```

#### Parameters

`reader` Utf8JsonReader&<br>

`typeToConvert` [Type](https://docs.microsoft.com/en-us/dotnet/api/system.type)<br>

`options` JsonSerializerOptions<br>

#### Returns

[List&lt;PostProcessEffect&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1)<br>

### **Write(Utf8JsonWriter, List&lt;PostProcessEffect&gt;, JsonSerializerOptions)**

```csharp
public void Write(Utf8JsonWriter writer, List<PostProcessEffect> value, JsonSerializerOptions options)
```

#### Parameters

`writer` Utf8JsonWriter<br>

`value` [List&lt;PostProcessEffect&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1)<br>

`options` JsonSerializerOptions<br>
