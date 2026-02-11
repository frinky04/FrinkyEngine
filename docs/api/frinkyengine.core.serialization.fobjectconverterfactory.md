# FObjectConverterFactory

Namespace: FrinkyEngine.Core.Serialization

JSON converter factory that handles serialization of [FObject](./frinkyengine.core.ecs.fobject) subclasses
 and [List&lt;T&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1) where T derives from [FObject](./frinkyengine.core.ecs.fobject).

```csharp
public class FObjectConverterFactory : System.Text.Json.Serialization.JsonConverterFactory
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → JsonConverter → JsonConverterFactory → [FObjectConverterFactory](./frinkyengine.core.serialization.fobjectconverterfactory)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **Type**

```csharp
public Type Type { get; }
```

#### Property Value

[Type](https://docs.microsoft.com/en-us/dotnet/api/system.type)<br>

## Constructors

### **FObjectConverterFactory()**

```csharp
public FObjectConverterFactory()
```

## Methods

### **CanConvert(Type)**

```csharp
public bool CanConvert(Type typeToConvert)
```

#### Parameters

`typeToConvert` [Type](https://docs.microsoft.com/en-us/dotnet/api/system.type)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **CreateConverter(Type, JsonSerializerOptions)**

```csharp
public JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
```

#### Parameters

`typeToConvert` [Type](https://docs.microsoft.com/en-us/dotnet/api/system.type)<br>

`options` JsonSerializerOptions<br>

#### Returns

JsonConverter<br>
