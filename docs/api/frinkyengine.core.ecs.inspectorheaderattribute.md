# InspectorHeaderAttribute

Namespace: FrinkyEngine.Core.ECS

Inserts a header label before the property.

```csharp
public sealed class InspectorHeaderAttribute : System.Attribute
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Attribute](https://docs.microsoft.com/en-us/dotnet/api/system.attribute) → [InspectorHeaderAttribute](./frinkyengine.core.ecs.inspectorheaderattribute)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute), [AttributeUsageAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.attributeusageattribute)

## Properties

### **Title**

Header text.

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

### **InspectorHeaderAttribute(String)**

Inserts a header label before the property.

```csharp
public InspectorHeaderAttribute(string title)
```

#### Parameters

`title` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
