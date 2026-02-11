# InspectorOnChangedAttribute

Namespace: FrinkyEngine.Core.ECS

Calls one or more methods after the property value changes in the inspector.

```csharp
public sealed class InspectorOnChangedAttribute : System.Attribute
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Attribute](https://docs.microsoft.com/en-us/dotnet/api/system.attribute) → [InspectorOnChangedAttribute](./frinkyengine.core.ecs.inspectoronchangedattribute)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute), [AttributeUsageAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.attributeusageattribute)

## Properties

### **MethodName**

Name of a parameterless instance method to invoke.

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

### **InspectorOnChangedAttribute(String)**

Calls one or more methods after the property value changes in the inspector.

```csharp
public InspectorOnChangedAttribute(string methodName)
```

#### Parameters

`methodName` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
