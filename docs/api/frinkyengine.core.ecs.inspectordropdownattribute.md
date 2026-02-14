# InspectorDropdownAttribute

Namespace: FrinkyEngine.Core.ECS

Renders an int property as a combo box dropdown. The named method must return `string[]` where each index maps to the int value.

```csharp
public sealed class InspectorDropdownAttribute : System.Attribute
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Attribute](https://docs.microsoft.com/en-us/dotnet/api/system.attribute) → [InspectorDropdownAttribute](./frinkyengine.core.ecs.inspectordropdownattribute)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute), [AttributeUsageAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.attributeusageattribute)

## Properties

### **MethodName**

Name of a parameterless instance method returning `string[]` of labels.

```csharp
public string MethodName { get; }
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

### **InspectorDropdownAttribute(String)**

Renders an int property as a combo box dropdown. The named method must return `string[]` where each index maps to the int value.

```csharp
public InspectorDropdownAttribute(string methodName)
```

#### Parameters

`methodName` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
