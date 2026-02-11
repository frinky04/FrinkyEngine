# ComponentData

Namespace: FrinkyEngine.Core.Serialization

JSON-serializable representation of a component, discriminated by the `$type` field.

```csharp
public class ComponentData
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ComponentData](./frinkyengine.core.serialization.componentdata)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **Type**

Fully qualified type name used by [ComponentTypeResolver](./frinkyengine.core.serialization.componenttyperesolver) for deserialization.

```csharp
public string Type { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **Enabled**

Whether the component is enabled.

```csharp
public bool Enabled { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **EditorOnly**

Whether the component is editor-only.

```csharp
public bool EditorOnly { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Properties**

Serialized public properties as key-value pairs of JSON elements.

```csharp
public Dictionary<string, JsonElement> Properties { get; set; }
```

#### Property Value

[Dictionary&lt;String, JsonElement&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2)<br>

## Constructors

### **ComponentData()**

```csharp
public ComponentData()
```
