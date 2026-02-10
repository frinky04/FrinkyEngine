# EntityReferenceConverter

Namespace: FrinkyEngine.Core.Serialization

JSON converter that serializes [EntityReference](./frinkyengine.core.ecs.entityreference) as a GUID string.

```csharp
public class EntityReferenceConverter : System.Text.Json.Serialization.JsonConverter`1[[FrinkyEngine.Core.ECS.EntityReference, FrinkyEngine.Core, Version=0.5.4.0, Culture=neutral, PublicKeyToken=null]]
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → JsonConverter → JsonConverter&lt;EntityReference&gt; → [EntityReferenceConverter](./frinkyengine.core.serialization.entityreferenceconverter)<br>
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

### **EntityReferenceConverter()**

```csharp
public EntityReferenceConverter()
```

## Methods

### **Read(Utf8JsonReader&, Type, JsonSerializerOptions)**

```csharp
public EntityReference Read(Utf8JsonReader& reader, Type typeToConvert, JsonSerializerOptions options)
```

#### Parameters

`reader` Utf8JsonReader&<br>

`typeToConvert` [Type](https://docs.microsoft.com/en-us/dotnet/api/system.type)<br>

`options` JsonSerializerOptions<br>

#### Returns

[EntityReference](./frinkyengine.core.ecs.entityreference)<br>

### **Write(Utf8JsonWriter, EntityReference, JsonSerializerOptions)**

```csharp
public void Write(Utf8JsonWriter writer, EntityReference value, JsonSerializerOptions options)
```

#### Parameters

`writer` Utf8JsonWriter<br>

`value` [EntityReference](./frinkyengine.core.ecs.entityreference)<br>

`options` JsonSerializerOptions<br>
