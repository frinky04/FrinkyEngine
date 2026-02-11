# InspectorSectionAttribute

Namespace: FrinkyEngine.Core.ECS

Groups a property under a labeled inspector section in the editor.

```csharp
public sealed class InspectorSectionAttribute : System.Attribute
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Attribute](https://docs.microsoft.com/en-us/dotnet/api/system.attribute) → [InspectorSectionAttribute](./frinkyengine.core.ecs.inspectorsectionattribute)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute), [AttributeUsageAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.attributeusageattribute)

## Properties

### **Title**

Section title shown above the property.

```csharp
public string Title { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **TypeId**

```csharp
public object TypeId { get; }
```

#### Property Value

[Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)<br>

## Constructors

### **InspectorSectionAttribute(String)**

Groups a property under a labeled inspector section in the editor.

```csharp
public InspectorSectionAttribute(string title)
```

#### Parameters

`title` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
