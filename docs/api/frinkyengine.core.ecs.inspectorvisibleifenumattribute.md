# InspectorVisibleIfEnumAttribute

Namespace: FrinkyEngine.Core.ECS

Shows the annotated property only when another enum property has a named value.

```csharp
public sealed class InspectorVisibleIfEnumAttribute : System.Attribute
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Attribute](https://docs.microsoft.com/en-us/dotnet/api/system.attribute) → [InspectorVisibleIfEnumAttribute](./frinkyengine.core.ecs.inspectorvisibleifenumattribute)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute), [AttributeUsageAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.attributeusageattribute)

## Properties

### **PropertyName**

Name of the sibling enum property used as the visibility condition source.

```csharp
public string PropertyName { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **ExpectedMemberName**

Required enum member name.

```csharp
public string ExpectedMemberName { get; }
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

### **InspectorVisibleIfEnumAttribute(String, String)**

Shows the annotated property only when another enum property has a named value.

```csharp
public InspectorVisibleIfEnumAttribute(string propertyName, string expectedMemberName)
```

#### Parameters

`propertyName` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`expectedMemberName` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
