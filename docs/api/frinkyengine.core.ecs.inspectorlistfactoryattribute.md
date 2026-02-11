# InspectorListFactoryAttribute

Namespace: FrinkyEngine.Core.ECS

Declares a factory method used when adding new elements to a reflected list.

```csharp
public sealed class InspectorListFactoryAttribute : System.Attribute
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Attribute](https://docs.microsoft.com/en-us/dotnet/api/system.attribute) → [InspectorListFactoryAttribute](./frinkyengine.core.ecs.inspectorlistfactoryattribute)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute), [AttributeUsageAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.attributeusageattribute)

## Properties

### **MethodName**

Name of a parameterless instance method used to create list elements.

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

### **InspectorListFactoryAttribute(String)**

Declares a factory method used when adding new elements to a reflected list.

```csharp
public InspectorListFactoryAttribute(string methodName)
```

#### Parameters

`methodName` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
