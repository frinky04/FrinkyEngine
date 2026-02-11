# InspectorVisibleIfAttribute

Namespace: FrinkyEngine.Core.ECS

Shows the annotated property only when another boolean property matches the expected value.

```csharp
public sealed class InspectorVisibleIfAttribute : System.Attribute
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Attribute](https://docs.microsoft.com/en-us/dotnet/api/system.attribute) → [InspectorVisibleIfAttribute](./frinkyengine.core.ecs.inspectorvisibleifattribute)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute), [AttributeUsageAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.attributeusageattribute)

## Properties

### **PropertyName**

Name of the sibling property used as the visibility condition source.

```csharp
public string PropertyName { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **ExpectedValue**

Expected boolean value required for visibility.

```csharp
public bool ExpectedValue { get; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **TypeId**

```csharp
public object TypeId { get; }
```

#### Property Value

[Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)<br>

## Constructors

### **InspectorVisibleIfAttribute(String, Boolean)**

Shows the annotated property only when another boolean property matches the expected value.

```csharp
public InspectorVisibleIfAttribute(string propertyName, bool expectedValue)
```

#### Parameters

`propertyName` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`expectedValue` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
